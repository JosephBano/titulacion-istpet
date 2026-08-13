using System;
using System.Collections.Generic;

namespace TitulacionIstpet.Domain.Entities;

public partial class NivelesAcademico
{
    public int IdNivelAcademico { get; set; }

    public string? Nombre { get; set; }

    public virtual ICollection<GradosAcademico> GradosAcademicos { get; set; } = [];
}
