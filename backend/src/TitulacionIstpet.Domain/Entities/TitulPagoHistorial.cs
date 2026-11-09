namespace TitulacionIstpet.Domain.Entities;

public partial class TitulPagoHistorial
{
    public int IdPagoHistorial { get; set; }
    public int IdPagoTitulacion { get; set; }
    public string? EstadoAnterior { get; set; }
    public string EstadoNuevo { get; set; } = string.Empty;
    public int IdUsuario { get; set; }
    public string? Motivo { get; set; }
    public DateTime FechaCambio { get; set; } = DateTime.UtcNow;

    public virtual TitulPago Pago { get; set; } = null!;
    public virtual Usuarios Usuario { get; set; } = null!;
}
