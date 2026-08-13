using System;
using System.Collections.Generic;

namespace TitulacionIstpet.Domain.Entities;

public partial class Vinculacionsubarea
{
    public int IdSubArea { get; set; }

    public int? IdArea { get; set; }

    public string? SubArea { get; set; }

    public bool Activo { get; set; }

    public virtual Vinculacionarea? IdAreaNavigation { get; set; }

    public virtual ICollection<Vinculacionsubareaespecifica> Vinculacionsubareaespecificas { get; set; } = new List<Vinculacionsubareaespecifica>();
}
