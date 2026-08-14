using System;
using System.Collections.Generic;

namespace TitulacionIstpet.Domain.Entities;

public partial class UsuariosWeb
{
    public string Usuario { get; set; } = null!;

    public string? Password { get; set; }

    public bool? Salida { get; set; }

    public bool? Ingreso { get; set; }

    public bool? Activo { get; set; }

    public bool? Asistencia { get; set; }

    public bool? EsRrhh { get; set; }
}
