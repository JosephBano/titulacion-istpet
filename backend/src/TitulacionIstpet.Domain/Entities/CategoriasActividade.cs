using System;
using System.Collections.Generic;

namespace TitulacionIstpet.Domain.Entities;

public partial class CategoriasActividade
{
    public int IdCategoria { get; set; }

    public string Categoria { get; set; } = null!;

    public bool? EsDocencia { get; set; }

    public bool? Activo { get; set; }

    public bool? Porcentaje { get; set; }

    public virtual ICollection<Seddinstrumento> Seddinstrumentos { get; set; } = new List<Seddinstrumento>();

    public virtual ICollection<SubcategoriasActividade> SubcategoriasActividades { get; set; } = new List<SubcategoriasActividade>();
}
