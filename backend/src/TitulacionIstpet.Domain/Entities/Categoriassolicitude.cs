using System;
using System.Collections.Generic;

namespace TitulacionIstpet.Domain.Entities;

public partial class Categoriassolicitude
{
    public int IdCategoriaSolicitud { get; set; }

    public string? Categoria { get; set; }

    public bool? Activo { get; set; }

    public virtual ICollection<Tipossolicitude> Tipossolicitudes { get; set; } = new List<Tipossolicitude>();
}
