using System;
using System.Collections.Generic;

namespace Titan.Domain.Entities;

public partial class v_alerta_mantenimiento
{
    public int id_vehiculo { get; set; }

    public string? numero_vehiculo { get; set; }

    public string? placa { get; set; }
}
