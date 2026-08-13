using System;
using System.Collections.Generic;

namespace TitulacionIstpet.Domain.Entities;

public partial class TiposBeca
{
    public int IdTipoBeca { get; set; }

    public string? Nombre { get; set; }

    public virtual ICollection<TitulosEnCurso> TitulosEnCursos { get; set; } = new List<TitulosEnCurso>();
}
