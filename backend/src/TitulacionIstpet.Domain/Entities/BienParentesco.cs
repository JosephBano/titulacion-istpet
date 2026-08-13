using System;
using System.Collections.Generic;

namespace TitulacionIstpet.Domain.Entities;

public partial class BienParentesco
{
    public int IdParentezco { get; set; }

    public string Nombre { get; set; } = null!;

    public bool EsPadre { get; set; }

    public bool EsMadre { get; set; }

    public virtual ICollection<BienParentezcosAlumno> BienParentezcosAlumnos { get; set; } = new List<BienParentezcosAlumno>();
}
