using System;
using System.Collections.Generic;

namespace TitulacionIstpet.Domain.Entities;

public partial class BienServiciosFicha
{
    public int IdServicioFicha { get; set; }

    public bool TieneServicio { get; set; }

    public int IdTipoServicio { get; set; }

    public int IdFichaSocioEconomica { get; set; }

    public virtual BienFichaSocioeconomica IdFichaSocioEconomicaNavigation { get; set; } = null!;

    public virtual BienTipoServicio IdTipoServicioNavigation { get; set; } = null!;
}
