using System;
using System.Collections.Generic;

namespace TitulacionIstpet.Domain.Entities;

public partial class FinanciamientoBeca
{
    public int IdFinanciamiento { get; set; }

    public string? Nombre { get; set; }

    public virtual ICollection<TitulosEnCurso> TitulosEnCursos { get; set; } = [];
}
