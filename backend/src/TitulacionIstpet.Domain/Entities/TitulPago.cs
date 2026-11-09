using TitulacionIstpet.Domain.Constants;
using TitulacionIstpet.Domain.Exceptions;

namespace TitulacionIstpet.Domain.Entities;

public partial class TitulPago
{
    public int IdPagoTitulacion { get; set; }
    public int IdPostulacionAlumno { get; set; }
    public int IdMatricula { get; set; }
    public int? IdConfiguracionPago { get; set; }
    public string TipoRubro { get; set; } = PagosConstantes.Rubros.Base;
    public string NumeroFactura { get; set; } = string.Empty;
    public decimal Valor { get; set; }
    public string Estado { get; set; } = PagosConstantes.Estados.Registrado;
    public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;
    public int IdUsuarioRegistro { get; set; }
    public string? Observacion { get; set; }
    public string? MotivoRechazo { get; set; }
    public DateTime? FechaRevision { get; set; }
    public int? IdUsuarioRevision { get; set; }
    public int? IdPagoSigafi { get; set; }
    public int? IdCredito { get; set; }
    public string? ComprobanteArchivo { get; set; }

    public virtual TitulPostulacionAlumnos PostulacionAlumno { get; set; } = null!;
    public virtual Matriculas Matricula { get; set; } = null!;
    public virtual TitulConfiguracionPagoModalidad? ConfiguracionPago { get; set; }
    public virtual Usuarios UsuarioRegistro { get; set; } = null!;
    public virtual Usuarios? UsuarioRevision { get; set; }
    public virtual Pagos? PagoSigafi { get; set; }
    public virtual ICollection<TitulPagoHistorial> Historial { get; set; } = new List<TitulPagoHistorial>();

    public bool CuentaComoPagado() => PagosConstantes.Estados.CuentaComoPagado(Estado);

    public void Aprobar(int idUsuarioContabilidad, string? observacion = null)
    {
        if (Estado == PagosConstantes.Estados.Rechazado)
        {
            throw new DominioException("Un pago rechazado previamente no puede ser aprobado directamente.");
        }

        Estado = PagosConstantes.Estados.Aprobado;
        IdUsuarioRevision = idUsuarioContabilidad;
        FechaRevision = DateTime.UtcNow;
        MotivoRechazo = null;

        Historial.Add(new TitulPagoHistorial
        {
            IdPagoTitulacion = IdPagoTitulacion,
            EstadoAnterior = PagosConstantes.Estados.Registrado,
            EstadoNuevo = PagosConstantes.Estados.Aprobado,
            IdUsuario = idUsuarioContabilidad,
            Motivo = observacion ?? "Pago aprobado por Contabilidad",
            FechaCambio = DateTime.UtcNow
        });
    }

    public void Rechazar(int idUsuarioContabilidad, string motivoRechazo)
    {
        if (string.IsNullOrWhiteSpace(motivoRechazo))
        {
            throw new DominioException("El motivo de rechazo es obligatorio para el área de Contabilidad.");
        }

        var estadoAnterior = Estado;
        Estado = PagosConstantes.Estados.Rechazado;
        IdUsuarioRevision = idUsuarioContabilidad;
        FechaRevision = DateTime.UtcNow;
        MotivoRechazo = motivoRechazo.Trim();

        Historial.Add(new TitulPagoHistorial
        {
            IdPagoTitulacion = IdPagoTitulacion,
            EstadoAnterior = estadoAnterior,
            EstadoNuevo = PagosConstantes.Estados.Rechazado,
            IdUsuario = idUsuarioContabilidad,
            Motivo = motivoRechazo.Trim(),
            FechaCambio = DateTime.UtcNow
        });
    }
}
