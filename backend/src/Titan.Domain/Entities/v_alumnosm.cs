using System;
using System.Collections.Generic;

namespace Titan.Domain.Entities;

public partial class v_alumnosm
{
    public int idmatricula { get; set; }

    public string idalumno { get; set; } = null!;

    public string? Nivel { get; set; }

    public string? seccion { get; set; }

    public string? modalidad { get; set; }

    public string idperiodo { get; set; } = null!;

    public string? paralelo { get; set; }

    public string? Estudiante { get; set; }

    public int idcarrera { get; set; }

    public string? carrera { get; set; }
}
