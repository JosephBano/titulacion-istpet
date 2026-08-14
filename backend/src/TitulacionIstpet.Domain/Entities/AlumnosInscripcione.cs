using System;
using System.Collections.Generic;

namespace TitulacionIstpet.Domain.Entities;

public partial class AlumnosInscripcione
{
    public int IdInscripcion { get; set; }

    public string? Idalumno { get; set; }

    public string? IdPeriodo { get; set; }

    public int? IdModalidad { get; set; }

    public int? IdNivel { get; set; }

    public int? IdSeccion { get; set; }

    public DateTime FechaRegistro { get; set; }

    public string? Usuario { get; set; }

    public bool? Activo { get; set; }

    public int? IdMedio { get; set; }
}
