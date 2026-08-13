using System;
using System.Collections.Generic;

namespace TitulacionIstpet.Domain.Entities;

public partial class BienGeneroAlumno
{
    public int IdGeneroAlumno { get; set; }

    public string Detalle { get; set; } = null!;

    public virtual ICollection<Alumno> Alumnos { get; set; } = [];
}
