using System;
using System.Collections.Generic;

namespace TitulacionIstpet.Domain.Entities;

public partial class Administrador
{
    public string IdAdministrador { get; set; } = null!;

    public string? NombresCompletos { get; set; }

    public string? ApellidosCompletos { get; set; }

    public string? Password { get; set; }

    public bool? EsAdministrador { get; set; }

    public DateOnly? FechaAsignacion { get; set; }

    public DateOnly? FechaModificacion { get; set; }

    public bool? EsActivo { get; set; }

    public bool? PrimerIngreso { get; set; }
}
