using System;
using System.Collections.Generic;

namespace TitulacionIstpet.Domain.Entities;

public partial class Paise
{
    public int Idpaises { get; set; }

    public string? Nombre { get; set; }

    public string? Nacionalidad { get; set; }

    public bool? EsEcuador { get; set; }

    public virtual ICollection<BienParentezcosAlumno> BienParentezcosAlumnos { get; set; } = new List<BienParentezcosAlumno>();

    public virtual ICollection<Provincia> Provincia { get; set; } = new List<Provincia>();

    public virtual ICollection<Universidade> Universidades { get; set; } = new List<Universidade>();
}
