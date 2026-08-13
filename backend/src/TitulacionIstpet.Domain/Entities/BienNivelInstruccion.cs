using System;
using System.Collections.Generic;

namespace TitulacionIstpet.Domain.Entities;

public partial class BienNivelInstruccion
{
    public int IdNivelInstruccion { get; set; }

    public string Detalle { get; set; } = null!;

    public virtual ICollection<BienParentezcosAlumno> BienParentezcosAlumnos { get; set; } = [];
}
