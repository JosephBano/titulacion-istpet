using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using TitulacionIstpet.Application.DTOs.Pagos;
using TitulacionIstpet.Domain.Constants;
using TitulacionIstpet.Domain.Entities;
using TitulacionIstpet.Domain.Exceptions;
using TitulacionIstpet.Infrastructure.Persistence;
using TitulacionIstpet.Infrastructure.Services;
using Xunit;

namespace TitulacionIstpet.Application.Tests.Features.Pagos;

public class PagosTitulacionServiceTests
{
    private static SigafiDbContext CrearContextoEnMemoria(string nombreBd)
    {
        var opciones = new DbContextOptionsBuilder<SigafiDbContext>()
            .UseInMemoryDatabase(databaseName: nombreBd)
            .Options;

        return new SigafiDbContext(opciones);
    }

    private static async Task<(SigafiDbContext context, int idPostulacionComplexivo, int idPostulacionPropuesta)> CrearEscenarioPruebaAsync(string dbName)
    {
        var context = CrearContextoEnMemoria(dbName);

        // 1. Modalidades
        var modComplexivo = new TitulModalidades
        {
            IdModalidadTitulacion = 1,
            ModalidadTitulacion = "Examen Complexivo",
            EsComplexivo = "1",
            GeneraTesis = "0",
            EsActivo = true
        };

        var modPropuesta = new TitulModalidades
        {
            IdModalidadTitulacion = 2,
            ModalidadTitulacion = "Propuesta Tecnológica",
            EsComplexivo = "0",
            GeneraTesis = "1",
            EsActivo = true
        };

        context.TitulModalidades.AddRange(modComplexivo, modPropuesta);

        // 2. Configuraciones de Pagos
        var cfgComplexivo = new TitulConfiguracionPagoModalidad
        {
            IdConfiguracionPago = 1,
            IdModalidadTitulacion = 1,
            ValorBase = 700.00m,
            MaximoInversion = 0.00m,
            TotalTitulacion = 700.00m,
            Activo = true
        };

        var cfgPropuesta = new TitulConfiguracionPagoModalidad
        {
            IdConfiguracionPago = 2,
            IdModalidadTitulacion = 2,
            ValorBase = 500.00m,
            MaximoInversion = 200.00m,
            TotalTitulacion = 700.00m,
            Activo = true
        };

        context.ConfiguracionesPagosModalidad.AddRange(cfgComplexivo, cfgPropuesta);

        // 3. Estudiante y Carreras
        var alumno = new Alumnos
        {
            IdAlumno = "1720000001",
            PrimerNombre = "Juan",
            SegundoNombre = "Carlos",
            ApellidoPaterno = "Pérez",
            ApellidoMaterno = "López"
        };
        context.Alumnos.Add(alumno);

        var carrera = new Carreras
        {
            IdCarrera = 10,
            Carrera = "Desarrollo de Software"
        };
        context.Carreras.Add(carrera);

        var modalidadCarrera = new ModalidadesCarreras
        {
            IdModalidadCarrera = 100,
            IdCarrera = 10,
            IdModalidad = 1,
            IdCarreraNavigation = carrera
        };
        context.ModalidadesCarreras.Add(modalidadCarrera);

        var cohorte = new TitulCohortes
        {
            IdCohorte = 1,
            IdPeriodo = "2026-S1",
            Detalle = "Cohorte 2026-S1",
            EsActivo = true
        };
        context.TitulCohortes.Add(cohorte);

        var cohorteCarrera = new TitulCohortesCarreras
        {
            IdCohorteCarrera = 50,
            IdCohorte = 1,
            IdModalidadCarrera = 100,
            IdCohorteNavigation = cohorte,
            IdModalidadCarreraNavigation = modalidadCarrera
        };
        context.TitulCohortesCarreras.Add(cohorteCarrera);

        var mtcComplexivo = new TitulModalidadesTitulacionCarreras
        {
            IdModalidadTitulacionCarrera = 501,
            IdModalidadTitulacion = 1,
            IdCohorteCarrera = 50,
            IdModalidadTitulacionNavigation = modComplexivo,
            IdCohorteCarreraNavigation = cohorteCarrera
        };

        var mtcPropuesta = new TitulModalidadesTitulacionCarreras
        {
            IdModalidadTitulacionCarrera = 502,
            IdModalidadTitulacion = 2,
            IdCohorteCarrera = 50,
            IdModalidadTitulacionNavigation = modPropuesta,
            IdCohorteCarreraNavigation = cohorteCarrera
        };

        context.TitulModalidadesTitulacionCarreras.AddRange(mtcComplexivo, mtcPropuesta);

        var matricula1 = new Matriculas { IdMatricula = 1001, IdAlumno = "1720000001", IdPeriodo = "2026-S1", IdAlumnoNavigation = alumno };
        var matricula2 = new Matriculas { IdMatricula = 1002, IdAlumno = "1720000001", IdPeriodo = "2026-S1", IdAlumnoNavigation = alumno };
        context.Matriculas.AddRange(matricula1, matricula2);

        var postComplexivo = new TitulPostulacionAlumnos
        {
            IdPostulacionAlumnos = 2001,
            IdMatricula = 1001,
            IdModalidadTitulacionCarrera = 501,
            IdPostulacionEstado = 1,
            EsActivo = true,
            IdMatriculaNavigation = matricula1,
            IdModalidadTitulacionCarreraNavigation = mtcComplexivo
        };

        var postPropuesta = new TitulPostulacionAlumnos
        {
            IdPostulacionAlumnos = 2002,
            IdMatricula = 1002,
            IdModalidadTitulacionCarrera = 502,
            IdPostulacionEstado = 1,
            EsActivo = true,
            IdMatriculaNavigation = matricula2,
            IdModalidadTitulacionCarreraNavigation = mtcPropuesta
        };

        context.TitulPostulacionAlumnos.AddRange(postComplexivo, postPropuesta);

        var usuario = new Usuarios
        {
            IdUsuario = 10,
            Nombre = "Secretaria General",
            IdSigafi = "0000000000",
            TablaSigafi = "usuarios",
            Contrasenia = "hash_pass",
            Activo = true
        };
        context.Usuarios.Add(usuario);

        await context.SaveChangesAsync();

        return (context, postComplexivo.IdPostulacionAlumnos, postPropuesta.IdPostulacionAlumnos);
    }

    [Fact]
    public async Task Complexivo_acepta_facturas_hasta_700_base_y_rechaza_por_encima()
    {
        var (context, idPostComplexivo, _) = await CrearEscenarioPruebaAsync(nameof(Complexivo_acepta_facturas_hasta_700_base_y_rechaza_por_encima));
        var sut = new PagosTitulacionService(context);

        // Acepta factura de $700 de valor base
        var pagoExitoso = await sut.RegistrarPagoAsync(new RegistrarPagoTitulacionDto(
            IdPostulacionAlumno: idPostComplexivo,
            TipoRubro: "BASE",
            NumeroFactura: "FAC-COMP-001",
            Valor: 700.00m
        ), idUsuario: 10);

        pagoExitoso.Valor.Should().Be(700.00m);
        pagoExitoso.Estado.Should().Be("REGISTRADO");

        // Rechaza un intento de ingresar más valor base cuando ya se copó
        var accion = () => sut.RegistrarPagoAsync(new RegistrarPagoTitulacionDto(
            IdPostulacionAlumno: idPostComplexivo,
            TipoRubro: "BASE",
            NumeroFactura: "FAC-COMP-002",
            Valor: 1.00m
        ), idUsuario: 10);

        await accion.Should().ThrowAsync<DominioException>()
            .WithMessage("*supera el saldo disponible*");
    }

    [Fact]
    public async Task Complexivo_rechaza_cualquier_valor_de_inversion()
    {
        var (context, idPostComplexivo, _) = await CrearEscenarioPruebaAsync(nameof(Complexivo_rechaza_cualquier_valor_de_inversion));
        var sut = new PagosTitulacionService(context);

        var accion = () => sut.RegistrarPagoAsync(new RegistrarPagoTitulacionDto(
            IdPostulacionAlumno: idPostComplexivo,
            TipoRubro: "INVERSION",
            NumeroFactura: "FAC-COMP-INV-001",
            Valor: 50.00m
        ), idUsuario: 10);

        await accion.Should().ThrowAsync<DominioException>()
            .WithMessage("*no admite rubros de inversión*");
    }

    [Fact]
    public async Task Propuesta_tecnologica_acepta_500_de_base_y_hasta_200_de_inversion()
    {
        var (context, _, idPostPropuesta) = await CrearEscenarioPruebaAsync(nameof(Propuesta_tecnologica_acepta_500_de_base_y_hasta_200_de_inversion));
        var sut = new PagosTitulacionService(context);

        // 1. Factura de Base por $500
        var pagoBase = await sut.RegistrarPagoAsync(new RegistrarPagoTitulacionDto(
            IdPostulacionAlumno: idPostPropuesta,
            TipoRubro: "BASE",
            NumeroFactura: "FAC-PROP-BASE-001",
            Valor: 500.00m
        ), idUsuario: 10);

        pagoBase.Valor.Should().Be(500.00m);
        pagoBase.Estado.Should().Be("REGISTRADO");

        // 2. Factura de Inversión por $200
        var pagoInversion = await sut.RegistrarPagoAsync(new RegistrarPagoTitulacionDto(
            IdPostulacionAlumno: idPostPropuesta,
            TipoRubro: "INVERSION",
            NumeroFactura: "FAC-PROP-INV-001",
            Valor: 200.00m
        ), idUsuario: 10);

        pagoInversion.Valor.Should().Be(200.00m);
        pagoInversion.Estado.Should().Be("REGISTRADO");

        var resumen = await sut.ObtenerResumenFinancieroEstudianteAsync(idPostPropuesta);
        resumen.TotalPagado.Should().Be(700.00m);
        resumen.SaldoTotalPendiente.Should().Be(0.00m);
        resumen.SaldoBaseDisponible.Should().Be(0.00m);
        resumen.SaldoInversionDisponible.Should().Be(0.00m);
    }

    [Fact]
    public async Task Propuesta_tecnologica_rechaza_201_de_inversion()
    {
        var (context, _, idPostPropuesta) = await CrearEscenarioPruebaAsync(nameof(Propuesta_tecnologica_rechaza_201_de_inversion));
        var sut = new PagosTitulacionService(context);

        var accion = () => sut.RegistrarPagoAsync(new RegistrarPagoTitulacionDto(
            IdPostulacionAlumno: idPostPropuesta,
            TipoRubro: "INVERSION",
            NumeroFactura: "FAC-PROP-INV-201",
            Valor: 201.00m
        ), idUsuario: 10);

        await accion.Should().ThrowAsync<DominioException>()
            .WithMessage("*supera el saldo disponible de $200.00 para inversión (Tope modalidad: $200.00, ya registrado: $0.00)*");
    }

    [Fact]
    public async Task Propuesta_tecnologica_rechaza_base_superior_a_500()
    {
        var (context, _, idPostPropuesta) = await CrearEscenarioPruebaAsync(nameof(Propuesta_tecnologica_rechaza_base_superior_a_500));
        var sut = new PagosTitulacionService(context);

        var accion = () => sut.RegistrarPagoAsync(new RegistrarPagoTitulacionDto(
            IdPostulacionAlumno: idPostPropuesta,
            TipoRubro: "BASE",
            NumeroFactura: "FAC-PROP-BASE-501",
            Valor: 501.00m
        ), idUsuario: 10);

        await accion.Should().ThrowAsync<DominioException>()
            .WithMessage("*supera el saldo disponible de $500.00 para el valor base*");
    }

    [Fact]
    public async Task No_permite_registrar_dos_veces_el_mismo_numero_de_factura()
    {
        var (context, _, idPostPropuesta) = await CrearEscenarioPruebaAsync(nameof(No_permite_registrar_dos_veces_el_mismo_numero_de_factura));
        var sut = new PagosTitulacionService(context);

        await sut.RegistrarPagoAsync(new RegistrarPagoTitulacionDto(
            IdPostulacionAlumno: idPostPropuesta,
            TipoRubro: "BASE",
            NumeroFactura: "FAC-UNICA-999",
            Valor: 300.00m
        ), idUsuario: 10);

        var accion = () => sut.RegistrarPagoAsync(new RegistrarPagoTitulacionDto(
            IdPostulacionAlumno: idPostPropuesta,
            TipoRubro: "BASE",
            NumeroFactura: "FAC-UNICA-999",
            Valor: 100.00m
        ), idUsuario: 10);

        await accion.Should().ThrowAsync<DominioException>()
            .WithMessage("*ya ha sido registrado previamente en el sistema*");
    }

    [Fact]
    public async Task Un_pago_registrado_aparece_como_pagado_de_inmediato()
    {
        var (context, _, idPostPropuesta) = await CrearEscenarioPruebaAsync(nameof(Un_pago_registrado_aparece_como_pagado_de_inmediato));
        var sut = new PagosTitulacionService(context);

        var pago = await sut.RegistrarPagoAsync(new RegistrarPagoTitulacionDto(
            IdPostulacionAlumno: idPostPropuesta,
            TipoRubro: "BASE",
            NumeroFactura: "FAC-INMEDIATA-01",
            Valor: 250.00m
        ), idUsuario: 10);

        pago.Estado.Should().Be("REGISTRADO");

        var resumen = await sut.ObtenerResumenFinancieroEstudianteAsync(idPostPropuesta);
        resumen.TotalPagado.Should().Be(250.00m);
        resumen.SaldoTotalPendiente.Should().Be(450.00m);
        resumen.SaldoBaseDisponible.Should().Be(250.00m);
    }

    [Fact]
    public async Task Un_pago_rechazado_por_contabilidad_deja_de_aparecer_como_pagado_y_libera_cupo()
    {
        var (context, _, idPostPropuesta) = await CrearEscenarioPruebaAsync(nameof(Un_pago_rechazado_por_contabilidad_deja_de_aparecer_como_pagado_y_libera_cupo));
        var sut = new PagosTitulacionService(context);

        // 1. Se registra factura de inversión por $200 (se copan los $200)
        var pago = await sut.RegistrarPagoAsync(new RegistrarPagoTitulacionDto(
            IdPostulacionAlumno: idPostPropuesta,
            TipoRubro: "INVERSION",
            NumeroFactura: "FAC-INV-RECHAZO-01",
            Valor: 200.00m
        ), idUsuario: 10);

        var resumenAntes = await sut.ObtenerResumenFinancieroEstudianteAsync(idPostPropuesta);
        resumenAntes.SaldoInversionDisponible.Should().Be(0.00m);

        // 2. Contabilidad rechaza el pago
        var pagoRechazado = await sut.ProcesarRevisionContabilidadAsync(
            pago.IdPagoTitulacion,
            new RevisionPagoDto(Aprobar: false, MotivoRechazo: "RUC no corresponde al proveedor de materiales"),
            idUsuarioContabilidad: 99
        );

        pagoRechazado.Estado.Should().Be("RECHAZADO");
        pagoRechazado.MotivoRechazo.Should().Be("RUC no corresponde al proveedor de materiales");

        // 3. El saldo de inversión se libera inmediatamente
        var resumenDespues = await sut.ObtenerResumenFinancieroEstudianteAsync(idPostPropuesta);
        resumenDespues.TotalInversionPagado.Should().Be(0.00m);
        resumenDespues.SaldoInversionDisponible.Should().Be(200.00m);

        // 4. Ahora sí se puede registrar un nuevo pago de inversión
        var nuevoPago = await sut.RegistrarPagoAsync(new RegistrarPagoTitulacionDto(
            IdPostulacionAlumno: idPostPropuesta,
            TipoRubro: "INVERSION",
            NumeroFactura: "FAC-INV-CORRECTA-02",
            Valor: 200.00m
        ), idUsuario: 10);

        nuevoPago.Estado.Should().Be("REGISTRADO");
    }

    [Fact]
    public async Task Contabilidad_rechazar_sin_motivo_es_rechazado_por_validacion()
    {
        var (context, _, idPostPropuesta) = await CrearEscenarioPruebaAsync(nameof(Contabilidad_rechazar_sin_motivo_es_rechazado_por_validacion));
        var sut = new PagosTitulacionService(context);

        var pago = await sut.RegistrarPagoAsync(new RegistrarPagoTitulacionDto(
            IdPostulacionAlumno: idPostPropuesta,
            TipoRubro: "BASE",
            NumeroFactura: "FAC-TEST-001",
            Valor: 100.00m
        ), idUsuario: 10);

        var accion = () => sut.ProcesarRevisionContabilidadAsync(
            pago.IdPagoTitulacion,
            new RevisionPagoDto(Aprobar: false, MotivoRechazo: ""),
            idUsuarioContabilidad: 99
        );

        await accion.Should().ThrowAsync<DominioException>()
            .WithMessage("*motivo de rechazo es obligatorio*");
    }

    [Fact]
    public async Task Cambiar_la_configuracion_de_una_modalidad_no_requiere_cambios_de_codigo()
    {
        var (context, _, idPostPropuesta) = await CrearEscenarioPruebaAsync(nameof(Cambiar_la_configuracion_de_una_modalidad_no_requiere_cambios_de_codigo));
        var sut = new PagosTitulacionService(context);

        // Se cambia la configuración a $600 de base y $100 de inversión dinámica
        var configuracionActualizada = await sut.GuardarConfiguracionAsync(new GuardarConfiguracionPagoModalidadDto(
            IdModalidadTitulacion: 2,
            IdPeriodo: null,
            ValorBase: 600.00m,
            MaximoInversion: 100.00m,
            TotalTitulacion: 700.00m,
            Activo: true
        ), idUsuario: 1);

        configuracionActualizada.ValorBase.Should().Be(600.00m);
        configuracionActualizada.MaximoInversion.Should().Be(100.00m);

        // Ahora registrar $150 de inversión falla porque el tope es $100
        var accion = () => sut.RegistrarPagoAsync(new RegistrarPagoTitulacionDto(
            IdPostulacionAlumno: idPostPropuesta,
            TipoRubro: "INVERSION",
            NumeroFactura: "FAC-DINAMICA-01",
            Valor: 150.00m
        ), idUsuario: 10);

        await accion.Should().ThrowAsync<DominioException>()
            .WithMessage("*supera el saldo disponible de $100.00 para inversión*");

        // Y registrar $600 de base es aceptado sin tocar código
        var pagoBase = await sut.RegistrarPagoAsync(new RegistrarPagoTitulacionDto(
            IdPostulacionAlumno: idPostPropuesta,
            TipoRubro: "BASE",
            NumeroFactura: "FAC-DINAMICA-02",
            Valor: 600.00m
        ), idUsuario: 10);

        pagoBase.Valor.Should().Be(600.00m);
    }
}
