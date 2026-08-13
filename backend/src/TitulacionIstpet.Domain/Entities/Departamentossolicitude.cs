using System;
using System.Collections.Generic;

namespace TitulacionIstpet.Domain.Entities;

public partial class Departamentossolicitude
{
    public int IdDepartamentoSolicitud { get; set; }

    public string? Departamento { get; set; }

    public bool? Activo { get; set; }

    public virtual ICollection<Tipossolicitude> Tipossolicitudes { get; set; } = new List<Tipossolicitude>();
}
