using System;
using System.Collections.Generic;

namespace TitulacionIstpet.Domain.Entities;

public partial class EdPregunta
{
    public int IdPregunta { get; set; }

    public int? IdEncuesta { get; set; }

    public string? Pregunta { get; set; }

    public int? Orden { get; set; }

    public bool? Activa { get; set; }

    public bool? EsAbierta { get; set; }
}
