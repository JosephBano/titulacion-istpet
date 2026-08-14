using System;
using System.Collections.Generic;

namespace TitulacionIstpet.Domain.Entities;

public partial class Sedddetallecoevaluacion
{
    public int IdDetalle { get; set; }

    public int? IdTest { get; set; }

    public int? IdPregunta { get; set; }

    public int? Respuesta { get; set; }

    public virtual Seddpregunta? IdPreguntaNavigation { get; set; }

    public virtual Seddcoevaluacion? IdTestNavigation { get; set; }
}
