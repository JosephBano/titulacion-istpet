using System;
using System.Collections.Generic;

namespace TitulacionIstpet.Domain.Entities;

public partial class AceptacionesUsuario
{
    public int IdAceptacionUsuario { get; set; }

    public string? IdUsuario { get; set; }

    public int? IdTermino { get; set; }

    public string? Sistema { get; set; }

    public DateTime? FechaRegistro { get; set; }

    public string? IpOrigen { get; set; }

    public string? Dispositivo { get; set; }
}
