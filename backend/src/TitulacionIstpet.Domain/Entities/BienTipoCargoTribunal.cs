using System;
using System.Collections.Generic;

namespace TitulacionIstpet.Domain.Entities;

public partial class BienTipoCargoTribunal
{
    public int IdTipoCargoTribunal { get; set; }

    public string Detalle { get; set; } = null!;

    public bool? EsRector { get; set; }

    public virtual ICollection<BienTribunal> BienTribunals { get; set; } = [];

    public virtual ICollection<BienVotosTribunale> BienVotosTribunales { get; set; } = [];
}
