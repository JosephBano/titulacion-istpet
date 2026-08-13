using System;
using System.Collections.Generic;

namespace TitulacionIstpet.Domain.Entities;

public partial class TiposAsignatura
{
    public int IdtipoAsignatura { get; set; }

    public string? TipoAsignatura { get; set; }

    public string? Abreviatura { get; set; }

    public bool? Activo { get; set; }

    public bool? NoDefinida { get; set; }

    public virtual ICollection<Detallemalla> Detallemallas { get; set; } = new List<Detallemalla>();
}
