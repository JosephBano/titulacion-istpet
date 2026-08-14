using System;
using System.Collections.Generic;

namespace TitulacionIstpet.Domain.Entities;

public partial class GestPasswordReset
{
    public int IdToken { get; set; }

    public int IdUsuario { get; set; }

    public string TokenHash { get; set; } = null!;

    public DateTime FechaCreacion { get; set; }

    public DateTime FechaExpiracion { get; set; }

    public bool Usado { get; set; }

    public string? IpSolicitud { get; set; }

    public virtual Usuario IdUsuarioNavigation { get; set; } = null!;
}
