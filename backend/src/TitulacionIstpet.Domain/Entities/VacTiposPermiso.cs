using System;
using System.Collections.Generic;

namespace TitulacionIstpet.Domain.Entities;

public partial class VacTiposPermiso
{
    public int IdTipoPermiso { get; set; }

    public string Nombre { get; set; } = null!;

    public string Unidad { get; set; } = null!;

    public bool RequiereAdjunto { get; set; }

    public bool AfectaVacaciones { get; set; }

    public decimal? MaxHorasPorSolicitud { get; set; }

    public int? MaxPermisosMes { get; set; }

    public bool EsCalamidadDomestica { get; set; }

    public int? DiasCalendarioFijos { get; set; }

    public bool EsRecuperable { get; set; }

    public bool? Activo { get; set; }
}
