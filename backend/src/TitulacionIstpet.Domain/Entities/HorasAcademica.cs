using System;
using System.Collections.Generic;

namespace TitulacionIstpet.Domain.Entities;

public partial class HorasAcademica
{
    public int IdHorasAcademicas { get; set; }

    public int IdDedicacion { get; set; }

    public int? HorasMinimas { get; set; }

    public int? HorasMaximas { get; set; }

    public int? HorasMaximaSemana { get; set; }

    public bool? EsActivo { get; set; }

    public virtual Dedicacion IdDedicacionNavigation { get; set; } = null!;
}
