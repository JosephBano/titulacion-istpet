using System;
using System.Collections.Generic;

namespace TitulacionIstpet.Domain.Entities;

public partial class Nacionalidade
{
    public int IdNacionalidad { get; set; }

    public string? Nacionalidad { get; set; }

    public bool? EsNinguna { get; set; }

    public virtual ICollection<Profesore> Profesores { get; set; } = new List<Profesore>();
}
