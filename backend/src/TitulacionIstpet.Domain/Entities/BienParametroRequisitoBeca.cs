using System;
using System.Collections.Generic;

namespace TitulacionIstpet.Domain.Entities;

public partial class BienParametroRequisitoBeca
{
    public int IdParametroRequisitoBeca { get; set; }

    public int IdRequistosBeca { get; set; }

    public bool EsObligatorio { get; set; }

    public bool EsActivo { get; set; }

    public int IdTipoApoyoFinanciero { get; set; }

    public virtual ICollection<BienPostulacionRequisitosBeca> BienPostulacionRequisitosBecas { get; set; } = [];

    public virtual BienRequisitosBeca IdRequistosBecaNavigation { get; set; } = null!;

    public virtual BienTipoApoyoFinanciero IdTipoApoyoFinancieroNavigation { get; set; } = null!;
}
