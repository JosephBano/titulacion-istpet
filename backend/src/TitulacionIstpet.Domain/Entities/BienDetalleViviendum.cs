using System;
using System.Collections.Generic;

namespace TitulacionIstpet.Domain.Entities;

public partial class BienDetalleViviendum
{
    public string IdAlumno { get; set; } = null!;

    public string TipoDeVivienda { get; set; } = null!;

    public int EspaciosFisicos { get; set; }

    public int Dormitorios { get; set; }

    public string Referencia { get; set; } = null!;

    public int? MiembrosHogar { get; set; }

    public int? AdultosVivienda { get; set; }

    public int? NiñosVivienda { get; set; }

    public bool? BonoDesarrolloHumano { get; set; }

    public decimal? IngresoPromedioHogar { get; set; }

    public int IdFichaSocioEconomica { get; set; }

    public virtual BienFichaSocioeconomica IdFichaSocioEconomicaNavigation { get; set; } = null!;
}
