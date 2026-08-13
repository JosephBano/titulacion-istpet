using System;
using System.Collections.Generic;

namespace TitulacionIstpet.Domain.Entities;

public partial class Vinculacionarea
{
    public int IdArea { get; set; }

    public string? Area { get; set; }

    public bool Activo { get; set; }

    public virtual ICollection<Vinculacionsubarea> Vinculacionsubareas { get; set; } = new List<Vinculacionsubarea>();
}
