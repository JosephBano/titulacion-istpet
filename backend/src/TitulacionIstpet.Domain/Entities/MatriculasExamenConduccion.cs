using System;
using System.Collections.Generic;

namespace TitulacionIstpet.Domain.Entities;

public partial class MatriculasExamenConduccion
{
    public int IdMatricula { get; set; }

    public int IdCategoria { get; set; }

    public int? Nota { get; set; }

    public string? Observacion { get; set; }

    public string? Usuario { get; set; }

    public DateOnly? FechaExamen { get; set; }

    public DateTime FechaIngreso { get; set; }

    public string? Instructor { get; set; }
}
