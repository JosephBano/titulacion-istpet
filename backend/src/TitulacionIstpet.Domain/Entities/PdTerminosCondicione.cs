using System;
using System.Collections.Generic;

namespace TitulacionIstpet.Domain.Entities;

public partial class PdTerminosCondicione
{
    public int IdTermino { get; set; }

    public int? IdCategoria { get; set; }

    public string? VersionTermino { get; set; }

    public string? Contenido { get; set; }

    public DateOnly? FechaPublicacion { get; set; }

    public DateTime FechaRegistro { get; set; }

    public string? ArchivoHtml { get; set; }

    public bool? EsVigente { get; set; }
}
