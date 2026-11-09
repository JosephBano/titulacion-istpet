using Microsoft.EntityFrameworkCore;
using TitulacionIstpet.Application.Common.Models;
using TitulacionIstpet.Application.DTOs.Pagos;
using TitulacionIstpet.Application.Interfaces;
using TitulacionIstpet.Domain.Constants;
using TitulacionIstpet.Domain.Entities;
using TitulacionIstpet.Domain.Exceptions;
using TitulacionIstpet.Infrastructure.Persistence;

namespace TitulacionIstpet.Infrastructure.Services;

public class PagosTitulacionService : IPagosTitulacionService
{
    private readonly SigafiDbContext _context;

    public PagosTitulacionService(SigafiDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<ConfiguracionPagoModalidadDto>> ListarConfiguracionesAsync(string? idPeriodo = null, CancellationToken ct = default)
    {
        var modalidades = await _context.TitulModalidades
            .AsNoTracking()
            .Where(m => m.EsActivo == true || m.EsActivo == null)
            .ToListAsync(ct);

        var query = _context.ConfiguracionesPagosModalidad
            .AsNoTracking()
            .Include(c => c.IdModalidadTitulacionNavigation)
            .Include(c => c.IdEspecieNavigation)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(idPeriodo))
        {
            query = query.Where(c => c.IdPeriodo == idPeriodo || c.IdPeriodo == null);
        }

        var configuraciones = await query.ToListAsync(ct);
        var resultado = new List<ConfiguracionPagoModalidadDto>();

        foreach (var mod in modalidades)
        {
            var config = configuraciones
                .Where(c => c.IdModalidadTitulacion == mod.IdModalidadTitulacion)
                .OrderByDescending(c => c.IdPeriodo != null)
                .FirstOrDefault();

            if (config != null)
            {
                resultado.Add(MapearConfiguracionDto(config));
            }
            else
            {
                // Fallback por defecto según tipo de modalidad
                var esComplexivo = string.Equals(mod.EsComplexivo, "1", StringComparison.OrdinalIgnoreCase) ||
                                  (mod.ModalidadTitulacion ?? string.Empty).Contains("complexivo", StringComparison.OrdinalIgnoreCase);

                decimal valorBase = esComplexivo ? 700.00m : 500.00m;
                decimal maxInversion = esComplexivo ? 0.00m : 200.00m;

                resultado.Add(new ConfiguracionPagoModalidadDto(
                    IdConfiguracionPago: 0,
                    IdModalidadTitulacion: mod.IdModalidadTitulacion,
                    ModalidadNombre: mod.ModalidadTitulacion ?? string.Empty,
                    IdPeriodo: idPeriodo,
                    ValorBase: valorBase,
                    MaximoInversion: maxInversion,
                    TotalTitulacion: PagosConstantes.TotalTitulacionOficial,
                    IdEspecie: null,
                    EspecieNombre: null,
                    Activo: true,
                    FechaRegistro: DateTime.UtcNow,
                    FechaModificacion: null
                ));
            }
        }

        return resultado;
    }

    public async Task<ConfiguracionPagoModalidadDto?> ObtenerConfiguracionPorModalidadAsync(int idModalidadTitulacion, string? idPeriodo = null, CancellationToken ct = default)
    {
        var modalidad = await _context.TitulModalidades
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.IdModalidadTitulacion == idModalidadTitulacion, ct);

        if (modalidad == null)
        {
            return null;
        }

        var config = await _context.ConfiguracionesPagosModalidad
            .AsNoTracking()
            .Include(c => c.IdModalidadTitulacionNavigation)
            .Include(c => c.IdEspecieNavigation)
            .Where(c => c.IdModalidadTitulacion == idModalidadTitulacion && c.Activo)
            .OrderByDescending(c => c.IdPeriodo == idPeriodo)
            .ThenByDescending(c => c.IdPeriodo != null)
            .FirstOrDefaultAsync(ct);

        if (config != null)
        {
            return MapearConfiguracionDto(config);
        }

        var esComplexivo = string.Equals(modalidad.EsComplexivo, "1", StringComparison.OrdinalIgnoreCase) ||
                          (modalidad.ModalidadTitulacion ?? string.Empty).Contains("complexivo", StringComparison.OrdinalIgnoreCase);

        decimal valorBase = esComplexivo ? 700.00m : 500.00m;
        decimal maxInversion = esComplexivo ? 0.00m : 200.00m;

        return new ConfiguracionPagoModalidadDto(
            IdConfiguracionPago: 0,
            IdModalidadTitulacion: modalidad.IdModalidadTitulacion,
            ModalidadNombre: modalidad.ModalidadTitulacion ?? string.Empty,
            IdPeriodo: idPeriodo,
            ValorBase: valorBase,
            MaximoInversion: maxInversion,
            TotalTitulacion: PagosConstantes.TotalTitulacionOficial,
            IdEspecie: null,
            EspecieNombre: null,
            Activo: true,
            FechaRegistro: DateTime.UtcNow,
            FechaModificacion: null
        );
    }

    public async Task<ConfiguracionPagoModalidadDto> GuardarConfiguracionAsync(GuardarConfiguracionPagoModalidadDto dto, int idUsuario, CancellationToken ct = default)
    {
        var modalidad = await _context.TitulModalidades
            .FirstOrDefaultAsync(m => m.IdModalidadTitulacion == dto.IdModalidadTitulacion, ct);

        if (modalidad == null)
        {
            throw new NoEncontradoException("Modalidad de Titulación", dto.IdModalidadTitulacion);
        }

        var config = await _context.ConfiguracionesPagosModalidad
            .FirstOrDefaultAsync(c => c.IdModalidadTitulacion == dto.IdModalidadTitulacion && c.IdPeriodo == dto.IdPeriodo, ct);

        if (config == null)
        {
            config = new TitulConfiguracionPagoModalidad
            {
                IdModalidadTitulacion = dto.IdModalidadTitulacion,
                IdPeriodo = string.IsNullOrWhiteSpace(dto.IdPeriodo) ? null : dto.IdPeriodo.Trim(),
                ValorBase = dto.ValorBase,
                MaximoInversion = dto.MaximoInversion,
                TotalTitulacion = dto.TotalTitulacion,
                IdEspecie = dto.IdEspecie,
                Activo = dto.Activo,
                FechaRegistro = DateTime.UtcNow
            };

            config.ValidarCoherencia();
            _context.ConfiguracionesPagosModalidad.Add(config);
        }
        else
        {
            config.ValorBase = dto.ValorBase;
            config.MaximoInversion = dto.MaximoInversion;
            config.TotalTitulacion = dto.TotalTitulacion;
            config.IdEspecie = dto.IdEspecie;
            config.Activo = dto.Activo;
            config.FechaModificacion = DateTime.UtcNow;

            config.ValidarCoherencia();
        }

        await _context.SaveChangesAsync(ct);

        // Recargar con navegaciones
        var configActualizada = await _context.ConfiguracionesPagosModalidad
            .AsNoTracking()
            .Include(c => c.IdModalidadTitulacionNavigation)
            .Include(c => c.IdEspecieNavigation)
            .FirstAsync(c => c.IdConfiguracionPago == config.IdConfiguracionPago, ct);

        return MapearConfiguracionDto(configActualizada);
    }

    public async Task<ResumenFinancieroEstudianteDto> ObtenerResumenFinancieroEstudianteAsync(int idPostulacionAlumno, CancellationToken ct = default)
    {
        var postulacion = await ObtenerPostulacionCompletaAsync(idPostulacionAlumno, ct);
        if (postulacion == null)
        {
            throw new NoEncontradoException("Postulación de Alumno", idPostulacionAlumno);
        }

        var idModalidad = postulacion.IdModalidadTitulacionCarreraNavigation.IdModalidadTitulacion;
        var idPeriodo = postulacion.IdModalidadTitulacionCarreraNavigation.IdCohorteCarreraNavigation?.IdCohorteNavigation?.IdPeriodo;
        var modalidadNombre = postulacion.IdModalidadTitulacionCarreraNavigation.IdModalidadTitulacionNavigation?.ModalidadTitulacion ?? "Modalidad";

        var config = await ObtenerConfiguracionPorModalidadAsync(idModalidad, idPeriodo, ct);
        decimal valorBaseMaximo = config?.ValorBase ?? 500.00m;
        decimal inversionMaxima = config?.MaximoInversion ?? 200.00m;
        decimal totalTitulacion = config?.TotalTitulacion ?? PagosConstantes.TotalTitulacionOficial;

        var pagos = await _context.PagosTitulacion
            .AsNoTracking()
            .Include(p => p.UsuarioRegistro)
            .Include(p => p.UsuarioRevision)
            .Where(p => p.IdPostulacionAlumno == idPostulacionAlumno)
            .OrderByDescending(p => p.FechaRegistro)
            .ToListAsync(ct);

        // Los pagos rechazados NO cuentan para los topes ni para lo pagado
        var pagosValidos = pagos.Where(p => p.CuentaComoPagado()).ToList();

        decimal totalBasePagado = pagosValidos
            .Where(p => string.Equals(p.TipoRubro, PagosConstantes.Rubros.Base, StringComparison.OrdinalIgnoreCase))
            .Sum(p => p.Valor);

        decimal totalInversionPagado = pagosValidos
            .Where(p => string.Equals(p.TipoRubro, PagosConstantes.Rubros.Inversion, StringComparison.OrdinalIgnoreCase))
            .Sum(p => p.Valor);

        decimal totalPagado = totalBasePagado + totalInversionPagado;
        decimal saldoBaseDisponible = Math.Max(0, valorBaseMaximo - totalBasePagado);
        decimal saldoInversionDisponible = Math.Max(0, inversionMaxima - totalInversionPagado);
        decimal saldoTotalPendiente = Math.Max(0, totalTitulacion - totalPagado);

        var alumno = postulacion.IdMatriculaNavigation?.IdAlumnoNavigation;
        var carrera = postulacion.IdModalidadTitulacionCarreraNavigation.IdCohorteCarreraNavigation?.IdModalidadCarreraNavigation?.IdCarreraNavigation;

        var pagosDto = pagos.Select(p => MapearPagoDto(p, postulacion)).ToList();

        return new ResumenFinancieroEstudianteDto(
            IdPostulacionAlumno: idPostulacionAlumno,
            IdMatricula: postulacion.IdMatricula,
            EstudianteCedula: alumno?.IdAlumno ?? string.Empty,
            EstudianteNombre: $"{alumno?.PrimerNombre} {alumno?.SegundoNombre} {alumno?.ApellidoPaterno} {alumno?.ApellidoMaterno}".Replace("  ", " ").Trim(),
            CarreraNombre: carrera?.Carrera ?? string.Empty,
            ModalidadNombre: modalidadNombre,
            IdModalidadTitulacion: idModalidad,
            IdPeriodo: idPeriodo,
            ValorBaseMaximo: valorBaseMaximo,
            InversionMaxima: inversionMaxima,
            TotalTitulacion: totalTitulacion,
            TotalBasePagado: totalBasePagado,
            TotalInversionPagado: totalInversionPagado,
            TotalPagado: totalPagado,
            SaldoBaseDisponible: saldoBaseDisponible,
            SaldoInversionDisponible: saldoInversionDisponible,
            SaldoTotalPendiente: saldoTotalPendiente,
            Pagos: pagosDto
        );
    }

    public async Task<PagoTitulacionDto> RegistrarPagoAsync(RegistrarPagoTitulacionDto dto, int idUsuario, CancellationToken ct = default)
    {
        if (dto.Valor <= 0)
        {
            throw new DominioException("El valor del comprobante de pago debe ser mayor a $0.00.");
        }

        if (string.IsNullOrWhiteSpace(dto.NumeroFactura))
        {
            throw new DominioException("El número de factura o comprobante es obligatorio.");
        }

        var rubroNormalizado = (dto.TipoRubro ?? string.Empty).Trim().ToUpperInvariant();
        if (!PagosConstantes.Rubros.EsValido(rubroNormalizado))
        {
            throw new DominioException($"El tipo de rubro '{dto.TipoRubro}' no es válido. Debe ser '{PagosConstantes.Rubros.Base}' o '{PagosConstantes.Rubros.Inversion}'.");
        }

        var facturaNormalizada = dto.NumeroFactura.Trim();

        // Control de concurrencia y validación atómica
        var postulacion = await ObtenerPostulacionCompletaAsync(dto.IdPostulacionAlumno, ct);
        if (postulacion == null)
        {
            throw new NoEncontradoException("Postulación de Alumno", dto.IdPostulacionAlumno);
        }

        var facturaExistente = await _context.PagosTitulacion
            .AsNoTracking()
            .AnyAsync(p => p.NumeroFactura == facturaNormalizada && p.Estado != PagosConstantes.Estados.Rechazado, ct);

        if (facturaExistente)
        {
            throw new DominioException($"El número de factura o comprobante '{facturaNormalizada}' ya ha sido registrado previamente en el sistema.");
        }

        var idModalidad = postulacion.IdModalidadTitulacionCarreraNavigation.IdModalidadTitulacion;
        var idPeriodo = postulacion.IdModalidadTitulacionCarreraNavigation.IdCohorteCarreraNavigation?.IdCohorteNavigation?.IdPeriodo;
        var modalidadNombre = postulacion.IdModalidadTitulacionCarreraNavigation.IdModalidadTitulacionNavigation?.ModalidadTitulacion ?? "Modalidad";

        var config = await ObtenerConfiguracionPorModalidadAsync(idModalidad, idPeriodo, ct);
        decimal valorBaseMaximo = config?.ValorBase ?? 500.00m;
        decimal inversionMaxima = config?.MaximoInversion ?? 200.00m;
        decimal totalTitulacion = config?.TotalTitulacion ?? PagosConstantes.TotalTitulacionOficial;

        var pagosValidos = await _context.PagosTitulacion
            .Where(p => p.IdPostulacionAlumno == dto.IdPostulacionAlumno &&
                       (p.Estado == PagosConstantes.Estados.Registrado || p.Estado == PagosConstantes.Estados.Aprobado))
            .ToListAsync(ct);

        decimal totalBasePagado = pagosValidos
            .Where(p => string.Equals(p.TipoRubro, PagosConstantes.Rubros.Base, StringComparison.OrdinalIgnoreCase))
            .Sum(p => p.Valor);

        decimal totalInversionPagado = pagosValidos
            .Where(p => string.Equals(p.TipoRubro, PagosConstantes.Rubros.Inversion, StringComparison.OrdinalIgnoreCase))
            .Sum(p => p.Valor);

        decimal totalPagado = totalBasePagado + totalInversionPagado;
        decimal saldoBaseDisponible = Math.Max(0, valorBaseMaximo - totalBasePagado);
        decimal saldoInversionDisponible = Math.Max(0, inversionMaxima - totalInversionPagado);
        decimal saldoTotalDisponible = Math.Max(0, totalTitulacion - totalPagado);

        // Validaciones de Topes
        if (rubroNormalizado == PagosConstantes.Rubros.Base)
        {
            if (dto.Valor > saldoBaseDisponible)
            {
                throw new DominioException(
                    $"El valor ingresado de ${dto.Valor:F2} supera el saldo disponible de ${saldoBaseDisponible:F2} para el valor base (Tope modalidad: ${valorBaseMaximo:F2}, ya registrado: ${totalBasePagado:F2}).");
            }
        }
        else if (rubroNormalizado == PagosConstantes.Rubros.Inversion)
        {
            if (inversionMaxima <= 0)
            {
                throw new DominioException(
                    $"La modalidad '{modalidadNombre}' no admite rubros de inversión (Tope de inversión permitido: $0.00, saldo disponible: $0.00).");
            }

            if (dto.Valor > saldoInversionDisponible)
            {
                throw new DominioException(
                    $"El valor ingresado de ${dto.Valor:F2} supera el saldo disponible de ${saldoInversionDisponible:F2} para inversión (Tope modalidad: ${inversionMaxima:F2}, ya registrado: ${totalInversionPagado:F2}).");
            }
        }

        if (dto.Valor > saldoTotalDisponible)
        {
            throw new DominioException(
                $"El valor ingresado de ${dto.Valor:F2} supera el saldo total de titulación disponible de ${saldoTotalDisponible:F2} (Total titulación: ${totalTitulacion:F2}, ya registrado: ${totalPagado:F2}).");
        }

        var nuevoPago = new TitulPago
        {
            IdPostulacionAlumno = dto.IdPostulacionAlumno,
            IdMatricula = postulacion.IdMatricula,
            IdConfiguracionPago = config?.IdConfiguracionPago > 0 ? config.IdConfiguracionPago : null,
            TipoRubro = rubroNormalizado,
            NumeroFactura = facturaNormalizada,
            Valor = dto.Valor,
            Estado = PagosConstantes.Estados.Registrado,
            FechaRegistro = DateTime.UtcNow,
            IdUsuarioRegistro = idUsuario,
            Observacion = string.IsNullOrWhiteSpace(dto.Observacion) ? null : dto.Observacion.Trim(),
            ComprobanteArchivo = string.IsNullOrWhiteSpace(dto.ComprobanteArchivo) ? null : dto.ComprobanteArchivo.Trim()
        };

        nuevoPago.Historial.Add(new TitulPagoHistorial
        {
            EstadoAnterior = null,
            EstadoNuevo = PagosConstantes.Estados.Registrado,
            IdUsuario = idUsuario,
            Motivo = "Registro inicial de comprobante de pago",
            FechaCambio = DateTime.UtcNow
        });

        // Sincronización segura con credito_alumno si existe una deuda registrada para la matrícula
        if (config?.IdEspecie != null)
        {
            var credito = await _context.CreditoAlumno
                .FirstOrDefaultAsync(c => c.IdMatricula == postulacion.IdMatricula && c.IdEspecie == config.IdEspecie.Value, ct);

            if (credito != null)
            {
                nuevoPago.IdCredito = credito.IdCredito;
                credito.Saldo = Math.Max(0, (credito.Saldo ?? 0) - dto.Valor);
            }
        }

        _context.PagosTitulacion.Add(nuevoPago);
        await _context.SaveChangesAsync(ct);

        // Recargar entidad con relaciones
        var pagoCreado = await _context.PagosTitulacion
            .AsNoTracking()
            .Include(p => p.UsuarioRegistro)
            .Include(p => p.UsuarioRevision)
            .FirstAsync(p => p.IdPagoTitulacion == nuevoPago.IdPagoTitulacion, ct);

        return MapearPagoDto(pagoCreado, postulacion);
    }

    public async Task<IReadOnlyList<PagoTitulacionDto>> ListarBandejaContabilidadAsync(string? estado = null, CancellationToken ct = default)
    {
        var query = _context.PagosTitulacion
            .AsNoTracking()
            .Include(p => p.PostulacionAlumno)
                .ThenInclude(pa => pa.IdMatriculaNavigation)
                    .ThenInclude(m => m.IdAlumnoNavigation)
            .Include(p => p.PostulacionAlumno)
                .ThenInclude(pa => pa.IdModalidadTitulacionCarreraNavigation)
                    .ThenInclude(mtc => mtc.IdModalidadTitulacionNavigation)
            .Include(p => p.PostulacionAlumno)
                .ThenInclude(pa => pa.IdModalidadTitulacionCarreraNavigation)
                    .ThenInclude(mtc => mtc.IdCohorteCarreraNavigation)
                        .ThenInclude(cc => cc.IdModalidadCarreraNavigation)
                            .ThenInclude(mc => mc.IdCarreraNavigation)
            .Include(p => p.UsuarioRegistro)
            .Include(p => p.UsuarioRevision)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(estado))
        {
            var estadoFiltro = estado.Trim().ToUpperInvariant();
            query = query.Where(p => p.Estado == estadoFiltro);
        }

        var pagos = await query.OrderByDescending(p => p.FechaRegistro).ToListAsync(ct);

        return pagos.Select(p => MapearPagoDto(p, p.PostulacionAlumno)).ToList();
    }

    public async Task<PagoTitulacionDto> ProcesarRevisionContabilidadAsync(int idPagoTitulacion, RevisionPagoDto revision, int idUsuarioContabilidad, CancellationToken ct = default)
    {
        var pago = await _context.PagosTitulacion
            .Include(p => p.Historial)
            .Include(p => p.PostulacionAlumno)
                .ThenInclude(pa => pa.IdMatriculaNavigation)
                    .ThenInclude(m => m.IdAlumnoNavigation)
            .Include(p => p.PostulacionAlumno)
                .ThenInclude(pa => pa.IdModalidadTitulacionCarreraNavigation)
                    .ThenInclude(mtc => mtc.IdModalidadTitulacionNavigation)
            .Include(p => p.PostulacionAlumno)
                .ThenInclude(pa => pa.IdModalidadTitulacionCarreraNavigation)
                    .ThenInclude(mtc => mtc.IdCohorteCarreraNavigation)
                        .ThenInclude(cc => cc.IdModalidadCarreraNavigation)
                            .ThenInclude(mc => mc.IdCarreraNavigation)
            .Include(p => p.UsuarioRegistro)
            .Include(p => p.UsuarioRevision)
            .FirstOrDefaultAsync(p => p.IdPagoTitulacion == idPagoTitulacion, ct);

        if (pago == null)
        {
            throw new NoEncontradoException("Pago de Titulación", idPagoTitulacion);
        }

        if (revision.Aprobar)
        {
            pago.Aprobar(idUsuarioContabilidad, revision.Observacion);
        }
        else
        {
            if (string.IsNullOrWhiteSpace(revision.MotivoRechazo))
            {
                throw new DominioException("El motivo de rechazo es obligatorio para el área de Contabilidad.");
            }

            pago.Rechazar(idUsuarioContabilidad, revision.MotivoRechazo);

            // Si estaba amortizando credito_alumno, restaurar el saldo al rechazar el pago
            if (pago.IdCredito != null)
            {
                var credito = await _context.CreditoAlumno.FindAsync([pago.IdCredito.Value], ct);
                if (credito != null)
                {
                    credito.Saldo = (credito.Saldo ?? 0) + pago.Valor;
                }
            }

            // Si tenía un pago sincronizado en tabla legacy pagos, marcar como anulado
            if (pago.IdPagoSigafi != null)
            {
                var pagoSigafi = await _context.Pagos.FindAsync([pago.IdPagoSigafi.Value], ct);
                if (pagoSigafi != null)
                {
                    pagoSigafi.Anulado = true;
                    pagoSigafi.FechaAnulacion = DateOnly.FromDateTime(DateTime.UtcNow);
                }
            }
        }

        await _context.SaveChangesAsync(ct);

        // Recargar datos actualizados
        var pagoActualizado = await _context.PagosTitulacion
            .AsNoTracking()
            .Include(p => p.UsuarioRegistro)
            .Include(p => p.UsuarioRevision)
            .FirstAsync(p => p.IdPagoTitulacion == idPagoTitulacion, ct);

        return MapearPagoDto(pagoActualizado, pago.PostulacionAlumno);
    }

    public async Task<IReadOnlyList<HistorialCambioEstadoPagoDto>> ObtenerHistorialPagoAsync(int idPagoTitulacion, CancellationToken ct = default)
    {
        var existe = await _context.PagosTitulacion.AnyAsync(p => p.IdPagoTitulacion == idPagoTitulacion, ct);
        if (!existe)
        {
            throw new NoEncontradoException("Pago de Titulación", idPagoTitulacion);
        }

        var historial = await _context.PagosTitulacionHistorial
            .AsNoTracking()
            .Include(h => h.Usuario)
            .Where(h => h.IdPagoTitulacion == idPagoTitulacion)
            .OrderBy(h => h.FechaCambio)
            .ToListAsync(ct);

        return historial.Select(h => new HistorialCambioEstadoPagoDto(
            IdPagoHistorial: h.IdPagoHistorial,
            IdPagoTitulacion: h.IdPagoTitulacion,
            EstadoAnterior: h.EstadoAnterior,
            EstadoNuevo: h.EstadoNuevo,
            IdUsuario: h.IdUsuario,
            UsuarioNombre: h.Usuario?.Nombre ?? $"Usuario #{h.IdUsuario}",
            Motivo: h.Motivo,
            FechaCambio: h.FechaCambio
        )).ToList();
    }

    private async Task<TitulPostulacionAlumnos?> ObtenerPostulacionCompletaAsync(int idPostulacionAlumno, CancellationToken ct)
    {
        return await _context.TitulPostulacionAlumnos
            .AsNoTracking()
            .Include(p => p.IdMatriculaNavigation)
                .ThenInclude(m => m.IdAlumnoNavigation)
            .Include(p => p.IdModalidadTitulacionCarreraNavigation)
                .ThenInclude(mtc => mtc.IdModalidadTitulacionNavigation)
            .Include(p => p.IdModalidadTitulacionCarreraNavigation)
                .ThenInclude(mtc => mtc.IdCohorteCarreraNavigation)
                    .ThenInclude(cc => cc.IdCohorteNavigation)
            .Include(p => p.IdModalidadTitulacionCarreraNavigation)
                .ThenInclude(mtc => mtc.IdCohorteCarreraNavigation)
                    .ThenInclude(cc => cc.IdModalidadCarreraNavigation)
                        .ThenInclude(mc => mc.IdCarreraNavigation)
            .FirstOrDefaultAsync(p => p.IdPostulacionAlumnos == idPostulacionAlumno, ct);
    }

    private static ConfiguracionPagoModalidadDto MapearConfiguracionDto(TitulConfiguracionPagoModalidad c) =>
        new(
            IdConfiguracionPago: c.IdConfiguracionPago,
            IdModalidadTitulacion: c.IdModalidadTitulacion,
            ModalidadNombre: c.IdModalidadTitulacionNavigation?.ModalidadTitulacion ?? string.Empty,
            IdPeriodo: c.IdPeriodo,
            ValorBase: c.ValorBase,
            MaximoInversion: c.MaximoInversion,
            TotalTitulacion: c.TotalTitulacion,
            IdEspecie: c.IdEspecie,
            EspecieNombre: c.IdEspecieNavigation?.Especie,
            Activo: c.Activo,
            FechaRegistro: c.FechaRegistro,
            FechaModificacion: c.FechaModificacion
        );

    private static PagoTitulacionDto MapearPagoDto(TitulPago p, TitulPostulacionAlumnos? postulacion)
    {
        var alumno = postulacion?.IdMatriculaNavigation?.IdAlumnoNavigation;
        var carrera = postulacion?.IdModalidadTitulacionCarreraNavigation?.IdCohorteCarreraNavigation?.IdModalidadCarreraNavigation?.IdCarreraNavigation;
        var modalidad = postulacion?.IdModalidadTitulacionCarreraNavigation?.IdModalidadTitulacionNavigation;

        return new PagoTitulacionDto(
            IdPagoTitulacion: p.IdPagoTitulacion,
            IdPostulacionAlumno: p.IdPostulacionAlumno,
            EstudianteCedula: alumno?.IdAlumno ?? string.Empty,
            EstudianteNombre: $"{alumno?.PrimerNombre} {alumno?.SegundoNombre} {alumno?.ApellidoPaterno} {alumno?.ApellidoMaterno}".Replace("  ", " ").Trim(),
            CarreraNombre: carrera?.Carrera ?? string.Empty,
            ModalidadNombre: modalidad?.ModalidadTitulacion ?? string.Empty,
            IdMatricula: p.IdMatricula,
            TipoRubro: p.TipoRubro,
            NumeroFactura: p.NumeroFactura,
            Valor: p.Valor,
            Estado: p.Estado,
            FechaRegistro: p.FechaRegistro,
            IdUsuarioRegistro: p.IdUsuarioRegistro,
            UsuarioRegistroNombre: p.UsuarioRegistro?.Nombre ?? $"Usuario #{p.IdUsuarioRegistro}",
            Observacion: p.Observacion,
            MotivoRechazo: p.MotivoRechazo,
            FechaRevision: p.FechaRevision,
            IdUsuarioRevision: p.IdUsuarioRevision,
            UsuarioRevisionNombre: p.UsuarioRevision?.Nombre,
            ComprobanteArchivo: p.ComprobanteArchivo
        );
    }
}
