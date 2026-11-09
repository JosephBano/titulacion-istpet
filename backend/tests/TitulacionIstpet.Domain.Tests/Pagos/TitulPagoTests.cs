using FluentAssertions;
using TitulacionIstpet.Domain.Constants;
using TitulacionIstpet.Domain.Entities;
using TitulacionIstpet.Domain.Exceptions;
using Xunit;

namespace TitulacionIstpet.Domain.Tests.Pagos;

public class TitulPagoTests
{
    [Fact]
    public void Pago_nuevo_se_crea_en_estado_registrado_y_cuenta_como_pagado()
    {
        var pago = new TitulPago
        {
            IdPostulacionAlumno = 10,
            IdMatricula = 50,
            NumeroFactura = "FAC-001",
            Valor = 500.00m,
            Estado = PagosConstantes.Estados.Registrado
        };

        pago.Estado.Should().Be("REGISTRADO");
        pago.CuentaComoPagado().Should().BeTrue();
    }

    [Fact]
    public void Aprobar_pago_actualiza_estado_y_mantiene_cuenta_como_pagado()
    {
        var pago = new TitulPago
        {
            IdPagoTitulacion = 1,
            IdPostulacionAlumno = 10,
            NumeroFactura = "FAC-001",
            Valor = 500.00m,
            Estado = PagosConstantes.Estados.Registrado
        };

        pago.Aprobar(idUsuarioContabilidad: 99, observacion: "Comprobante verificado");

        pago.Estado.Should().Be("APROBADO");
        pago.IdUsuarioRevision.Should().Be(99);
        pago.FechaRevision.Should().NotBeNull();
        pago.CuentaComoPagado().Should().BeTrue();
        pago.Historial.Should().ContainSingle(h => h.EstadoNuevo == "APROBADO" && h.IdUsuario == 99);
    }

    [Fact]
    public void Rechazar_pago_requiere_motivo_obligatorio()
    {
        var pago = new TitulPago
        {
            IdPagoTitulacion = 1,
            Estado = PagosConstantes.Estados.Registrado
        };

        var accion = () => pago.Rechazar(idUsuarioContabilidad: 99, motivoRechazo: "   ");

        accion.Should().Throw<DominioException>()
            .WithMessage("*motivo de rechazo es obligatorio*");
    }

    [Fact]
    public void Rechazar_pago_hace_que_deje_de_contar_como_pagado_y_libera_cupo()
    {
        var pago = new TitulPago
        {
            IdPagoTitulacion = 1,
            NumeroFactura = "FAC-001",
            Valor = 200.00m,
            Estado = PagosConstantes.Estados.Registrado
        };

        pago.Rechazar(idUsuarioContabilidad: 99, motivoRechazo: "Factura ilegible");

        pago.Estado.Should().Be("RECHAZADO");
        pago.MotivoRechazo.Should().Be("Factura ilegible");
        pago.CuentaComoPagado().Should().BeFalse();
        pago.Historial.Should().ContainSingle(h => h.EstadoNuevo == "RECHAZADO" && h.Motivo == "Factura ilegible");
    }

    [Fact]
    public void Intentar_aprobar_pago_rechazado_arroja_excepcion()
    {
        var pago = new TitulPago
        {
            IdPagoTitulacion = 1,
            Estado = PagosConstantes.Estados.Rechazado
        };

        var accion = () => pago.Aprobar(idUsuarioContabilidad: 99);

        accion.Should().Throw<DominioException>()
            .WithMessage("*rechazado previamente no puede ser aprobado*");
    }
}
