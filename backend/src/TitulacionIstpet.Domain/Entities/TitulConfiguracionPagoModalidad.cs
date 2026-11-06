using TitulacionIstpet.Domain.Constants;
using TitulacionIstpet.Domain.Exceptions;

namespace TitulacionIstpet.Domain.Entities;

public partial class TitulConfiguracionPagoModalidad
{
    public int IdConfiguracionPago { get; set; }
    public int IdModalidadTitulacion { get; set; }
    public string? IdPeriodo { get; set; }
    public decimal ValorBase { get; set; }
    public decimal MaximoInversion { get; set; }
    public decimal TotalTitulacion { get; set; } = PagosConstantes.TotalTitulacionOficial;
    public int? IdEspecie { get; set; }
    public bool Activo { get; set; } = true;
    public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;
    public DateTime? FechaModificacion { get; set; }

    public virtual TitulModalidades IdModalidadTitulacionNavigation { get; set; } = null!;
    public virtual Especies? IdEspecieNavigation { get; set; }

    public void ValidarCoherencia()
    {
        if (ValorBase < 0)
        {
            throw new DominioException("El valor base de titulación no puede ser negativo.");
        }

        if (MaximoInversion < 0)
        {
            throw new DominioException("El monto máximo de inversión no puede ser negativo.");
        }

        if (MaximoInversion > PagosConstantes.TopeMaximoInversion)
        {
            throw new DominioException($"El monto máximo de inversión no puede superar ${PagosConstantes.TopeMaximoInversion:F2}.");
        }

        if (TotalTitulacion != PagosConstantes.TotalTitulacionOficial)
        {
            throw new DominioException($"El costo total de la titulación debe ser ${PagosConstantes.TotalTitulacionOficial:F2}.");
        }

        if (ValorBase + MaximoInversion != TotalTitulacion)
        {
            throw new DominioException($"La suma del valor base (${ValorBase:F2}) y la inversión máxima (${MaximoInversion:F2}) debe coincidir con el total de titulación (${TotalTitulacion:F2}).");
        }
    }
}
