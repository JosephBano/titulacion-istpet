using System;
using System.Collections.Generic;

namespace TitulacionIstpet.Domain.Entities;

public partial class DetalleSistemaEvaluacion
{
    public string Idperiodo { get; set; } = null!;

    public int Idcarrera { get; set; }

    public int Idsistemaevaluacion { get; set; }
}
