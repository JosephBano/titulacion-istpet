using System;
using System.Collections.Generic;

namespace TitulacionIstpet.Domain.Entities;

public partial class Vinculacionsubareaespecifica
{
    public int IdSubAreaEspecifica { get; set; }

    public int? IdSubArea { get; set; }

    public string? SubAreaEspecifica { get; set; }

    public bool Activo { get; set; }

    public virtual Vinculacionsubarea? IdSubAreaNavigation { get; set; }
}
