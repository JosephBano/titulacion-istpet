using System;
using System.Collections.Generic;

namespace TitulacionIstpet.Domain.Entities;

public partial class EdRespuestastestab
{
    public int IdIngresoTest { get; set; }

    public int IdPregunta { get; set; }

    public string? Respuesta { get; set; }
}
