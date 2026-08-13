using System;
using System.Collections.Generic;

namespace TitulacionIstpet.Domain.Entities;

public partial class CarrerasAdjunto
{
    public int IdCarrerasAdjuntos { get; set; }

    public int IdCarrera { get; set; }

    public int IdAdjuntosImagenes { get; set; }

    public virtual AdjuntosImagene IdAdjuntosImagenesNavigation { get; set; } = null!;

    public virtual Carrera IdCarreraNavigation { get; set; } = null!;
}
