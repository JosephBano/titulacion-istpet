using System;
using System.Collections.Generic;

namespace TitulacionIstpet.Domain.Entities;

public partial class Vinculacionproyectosplanesaprendizaje
{
    public int IdProyectosPlanesAprendizaje { get; set; }

    public int? IdProyectoVinculacion { get; set; }

    public int? IdProyectosResultadosAprendizaje { get; set; }

    public string? Actividad { get; set; }

    public DateTime? FechaRegistro { get; set; }

    public bool Activo { get; set; }

    public virtual Vinculacionproyecto? IdProyectoVinculacionNavigation { get; set; }

    public virtual Vinculacionproyectosresultadosaprendizaje? IdProyectosResultadosAprendizajeNavigation { get; set; }
}
