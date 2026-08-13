using System;
using System.Collections.Generic;

namespace TitulacionIstpet.Domain.Entities;

public partial class BienTipoDesarrollo
{
    public int IdTipoDesarrollo { get; set; }

    public string Detalle { get; set; } = null!;

    public bool EsCierre { get; set; }

    public bool EsActivo { get; set; }

    public virtual ICollection<BienCasoDesarrollo> BienCasoDesarrollos { get; set; } = new List<BienCasoDesarrollo>();
}
