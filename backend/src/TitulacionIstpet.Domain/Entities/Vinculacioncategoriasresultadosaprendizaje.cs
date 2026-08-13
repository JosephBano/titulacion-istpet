using System;
using System.Collections.Generic;

namespace TitulacionIstpet.Domain.Entities;

public partial class Vinculacioncategoriasresultadosaprendizaje
{
    public int IdCategoriaResultadoAprendizaje { get; set; }

    public string? CategoriaResultadoAprendizaje { get; set; }

    public bool? Activo { get; set; }

    public virtual ICollection<Vinculacionproyectosresultadosaprendizaje> Vinculacionproyectosresultadosaprendizajes { get; set; } = [];
}
