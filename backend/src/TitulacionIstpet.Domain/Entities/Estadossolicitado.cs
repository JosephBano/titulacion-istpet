using System;
using System.Collections.Generic;

namespace TitulacionIstpet.Domain.Entities;

public partial class Estadossolicitado
{
    public int IdEstadoSolicitud { get; set; }

    public string? Estado { get; set; }

    public int? Orden { get; set; }

    public bool? EsTerminal { get; set; }

    public bool? EsPendiente { get; set; }

    public bool? EsFinalizado { get; set; }

    public bool? EsEnRevision { get; set; }

    public bool? EsAnulada { get; set; }

    public bool? EsReasignada { get; set; }
}
