using System;
using System.Collections.Generic;

namespace TitulacionIstpet.Domain.Entities;

public partial class BienRequisitosBeca
{
    public int IdRequistosBeca { get; set; }

    public string Requisito { get; set; } = null!;

    public string TipoRequisito { get; set; } = null!;

    public bool EsActivo { get; set; }

    public virtual ICollection<BienParametroRequisitoBeca> BienParametroRequisitoBecas { get; set; } = [];
}
