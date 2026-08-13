using System;
using System.Collections.Generic;

namespace TitulacionIstpet.Domain.Entities;

public partial class SubcategoriasActividade
{
    public int IdSubcategoria { get; set; }

    public int? IdCategoria { get; set; }

    public string? Subcategoria { get; set; }

    public bool? EsDocencia { get; set; }

    public bool? Activa { get; set; }

    public virtual CategoriasActividade? IdCategoriaNavigation { get; set; }

    public virtual ICollection<ProfesoresActividade> ProfesoresActividades { get; set; } = new List<ProfesoresActividade>();
}
