using System;
using System.Collections.Generic;

namespace TitulacionIstpet.Domain.Entities;

public partial class BienCasoDesarrolloDoc
{
    public int IdCasoDesarrolloDoc { get; set; }

    public int IdCasoDesarrollo { get; set; }

    public int IdAdjuntosImagenes { get; set; }

    public virtual AdjuntosImagene IdAdjuntosImagenesNavigation { get; set; } = null!;

    public virtual BienCasoDesarrollo IdCasoDesarrolloNavigation { get; set; } = null!;
}
