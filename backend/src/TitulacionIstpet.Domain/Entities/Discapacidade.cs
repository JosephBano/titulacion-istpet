using System;
using System.Collections.Generic;

namespace TitulacionIstpet.Domain.Entities;

public partial class Discapacidade
{
    public int IdDiscapacidad { get; set; }

    public string? Discapacidad { get; set; }

    public bool? EsDefecto { get; set; }

    public virtual ICollection<Profesore> Profesores { get; set; } = [];
}
