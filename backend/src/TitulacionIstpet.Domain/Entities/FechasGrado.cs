using System;
using System.Collections.Generic;

namespace TitulacionIstpet.Domain.Entities;

public partial class FechasGrado
{
    public string Idperiodo { get; set; } = null!;

    public int Idnivel { get; set; }

    public int Idseccion { get; set; }

    public string Paralelo { get; set; } = null!;

    public DateOnly? FechaGrado { get; set; }
}
