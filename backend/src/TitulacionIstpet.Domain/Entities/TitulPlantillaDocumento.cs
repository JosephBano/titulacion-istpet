namespace TitulacionIstpet.Domain.Entities;

public class TitulPlantillaDocumento
{
    public int IdPlantillaDocumento { get; set; }

    public string Codigo { get; set; } = null!;

    public string DefinicionJson { get; set; } = null!;

    public int Revision { get; set; } = 1;

    public int? ActualizadoPor { get; set; }

    public DateTime FechaCreacion { get; set; }

    public DateTime FechaActualizacion { get; set; }

    public virtual Usuarios? ActualizadoPorNavigation { get; set; }
}
