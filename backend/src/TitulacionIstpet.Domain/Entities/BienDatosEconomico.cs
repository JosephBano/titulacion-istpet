using System;
using System.Collections.Generic;

namespace TitulacionIstpet.Domain.Entities;

public partial class BienDatosEconomico
{
    public int IdFichaSocioEconomica { get; set; }

    public bool FamiliaRecibeBono { get; set; }

    public string TipoActividadEconomica { get; set; } = null!;

    public decimal IngresosPropios { get; set; }

    public string? EmpleaIngresos { get; set; }

    public string? NombreBono { get; set; }

    public virtual BienFichaSocioeconomica IdFichaSocioEconomicaNavigation { get; set; } = null!;
}
