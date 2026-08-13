using System;
using System.Collections.Generic;

namespace TitulacionIstpet.Domain.Entities;

public partial class BienMotivosBeca
{
    public int IdMotivosBeca { get; set; }

    public bool EsActivo { get; set; }

    public string? MotivoBeca { get; set; }

    public bool? EsDefault { get; set; }

    public int IdTipoApoyoFinanciero { get; set; }

    public int IdPorcentajeBeca { get; set; }

    public virtual ICollection<BienPostulacionesBeca> BienPostulacionesBecas { get; set; } = new List<BienPostulacionesBeca>();

    public virtual BienPorcentajeBeca IdPorcentajeBecaNavigation { get; set; } = null!;

    public virtual BienTipoApoyoFinanciero IdTipoApoyoFinancieroNavigation { get; set; } = null!;
}
