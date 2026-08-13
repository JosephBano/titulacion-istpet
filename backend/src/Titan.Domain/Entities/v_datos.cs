using System;
using System.Collections.Generic;

namespace Titan.Domain.Entities;

public partial class v_datos
{
    public string idAlumno { get; set; } = null!;

    public string? Datos { get; set; }

    public string? clave { get; set; }

    public string Tipo { get; set; } = null!;
}
