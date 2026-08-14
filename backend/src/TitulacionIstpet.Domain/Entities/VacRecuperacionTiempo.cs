using System;
using System.Collections.Generic;

namespace TitulacionIstpet.Domain.Entities;

public partial class VacRecuperacionTiempo
{
    public int IdRecuperacion { get; set; }

    public int IdPermiso { get; set; }

    public DateOnly FechaRecuperada { get; set; }

    public decimal HorasRecuperadas { get; set; }

    public DateTime FechaRegistro { get; set; }

    public int UsuarioTh { get; set; }

    public virtual VacPermiso IdPermisoNavigation { get; set; } = null!;
}
