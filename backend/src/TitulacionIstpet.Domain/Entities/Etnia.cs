using System;
using System.Collections.Generic;

namespace TitulacionIstpet.Domain.Entities;

public partial class Etnia
{
    public int IdEtnia { get; set; }

    public string? Etnia1 { get; set; }

    public bool? EsIndigena { get; set; }

    public bool? NoRegistra { get; set; }

    public virtual ICollection<Alumno> Alumnos { get; set; } = new List<Alumno>();

    public virtual ICollection<Profesore> Profesores { get; set; } = new List<Profesore>();
}
