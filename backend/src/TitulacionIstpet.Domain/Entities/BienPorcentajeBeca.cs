using System;
using System.Collections.Generic;

namespace TitulacionIstpet.Domain.Entities;

public partial class BienPorcentajeBeca
{
    public int IdPorcentajeBeca { get; set; }

    public decimal Porcentaje { get; set; }

    public bool EsActivo { get; set; }

    public virtual ICollection<BienMotivosBeca> BienMotivosBecas { get; set; } = new List<BienMotivosBeca>();
}
