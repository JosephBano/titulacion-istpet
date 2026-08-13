using System;
using System.Collections.Generic;

namespace TitulacionIstpet.Domain.Entities;

public partial class EdRespuestastest
{
    public int IdIngresoTest { get; set; }

    public int IdPregunta { get; set; }

    public bool? Siempre { get; set; }

    public bool? CasiSiempre { get; set; }

    public bool? AVeces { get; set; }

    public bool? CasiNunca { get; set; }
}
