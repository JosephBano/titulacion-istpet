using System;
using System.Collections.Generic;

namespace TitulacionIstpet.Domain.Entities;

public partial class AsignacionesProfesoresGrado
{
    public string IdProfesor { get; set; } = null!;

    public int IdAsignatura { get; set; }

    public string IdPeriodo { get; set; } = null!;

    public int IdModalidad { get; set; }

    public int IdSeccion { get; set; }

    public int IdNivel { get; set; }

    public string Paralelo { get; set; } = null!;

    public bool? Activo { get; set; }
}
