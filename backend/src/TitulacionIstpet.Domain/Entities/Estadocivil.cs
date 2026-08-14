using System;
using System.Collections.Generic;

namespace TitulacionIstpet.Domain.Entities;

public partial class Estadocivil
{
    public int IdestadoCivil { get; set; }

    public string? Nombre { get; set; }

    public bool? RequiereConyuge { get; set; }

    public virtual ICollection<Alumno> Alumnos { get; set; } = [];

    public virtual ICollection<Profesore> Profesores { get; set; } = [];
}
