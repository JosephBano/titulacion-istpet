using System;
using System.Collections.Generic;

namespace TitulacionIstpet.Domain.Entities;

public partial class Vinculacionproyectoscronograma
{
    public int IdProyectosCronograma { get; set; }

    public int? IdProyectoVinculacion { get; set; }

    public DateTime? FechaInicioPlanificada { get; set; }

    public DateTime? FechaFinPlanificada { get; set; }

    public DateTime? FechaInicioCumplida { get; set; }

    public DateTime? FechaFinCumplida { get; set; }

    public string? Actividad { get; set; }

    public DateTime? FechaRegistro { get; set; }

    public bool Activo { get; set; }

    public virtual Vinculacionproyecto? IdProyectoVinculacionNavigation { get; set; }
}
