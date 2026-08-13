using System;
using System.Collections.Generic;

namespace TitulacionIstpet.Domain.Entities;

public partial class Vinculacionproyectosresultadosaprendizaje
{
    public int IdProyectosResultadosAprendizaje { get; set; }

    public int? IdProyectoVinculacion { get; set; }

    public int? IdCategoriaResultadoAprendizaje { get; set; }

    public string? Resultado { get; set; }

    public DateTime? FechaRegistro { get; set; }

    public bool Activo { get; set; }

    public virtual Vinculacioncategoriasresultadosaprendizaje? IdCategoriaResultadoAprendizajeNavigation { get; set; }

    public virtual Vinculacionproyecto? IdProyectoVinculacionNavigation { get; set; }

    public virtual ICollection<Vinculacionproyectosplanesaprendizaje> Vinculacionproyectosplanesaprendizajes { get; set; } = [];
}
