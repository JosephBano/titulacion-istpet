using System;
using System.Collections.Generic;

namespace TitulacionIstpet.Domain.Entities;

public partial class Seccione
{
    public int IdSeccion { get; set; }

    public string? Seccion { get; set; }

    public string? Sufijo { get; set; }

    public virtual ICollection<Matricula> Matriculas { get; set; } = [];
}
