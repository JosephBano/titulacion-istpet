using System;
using System.Collections.Generic;

namespace Titan.Domain.Entities;

public partial class v_clases_activas
{
    public int id_registro { get; set; }

    public string idAlumno { get; set; } = null!;

    public string? primer_nombre { get; set; }

    public string? apellido_paterno { get; set; }

    public string? estudiante { get; set; }

    public int? id_vehiculo { get; set; }

    public string? numero_vehiculo { get; set; }

    public string? placa { get; set; }

    public string? instructor { get; set; }

    public TimeOnly? salida { get; set; }
}
