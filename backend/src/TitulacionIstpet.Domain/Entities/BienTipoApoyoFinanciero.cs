using System;
using System.Collections.Generic;

namespace TitulacionIstpet.Domain.Entities;

public partial class BienTipoApoyoFinanciero
{
    public int IdTipoApoyoFinanciero { get; set; }

    public int? IdBienApoyo { get; set; }

    public string NombreApoyo { get; set; } = null!;

    public bool EsActivo { get; set; }

    public virtual ICollection<BienMotivosBeca> BienMotivosBecas { get; set; } = [];

    public virtual ICollection<BienParametroRequisitoBeca> BienParametroRequisitoBecas { get; set; } = [];
}
