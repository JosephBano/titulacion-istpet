using System;
using System.Collections.Generic;

namespace TitulacionIstpet.Domain.Entities;

public partial class Solicitudeslog
{
    public int IdLogSolicitud { get; set; }

    public int? IdSolicitud { get; set; }

    public DateTime? FechaRegistro { get; set; }

    public string? Detalle { get; set; }

    public int? IdRespuestaSolicitud { get; set; }
}
