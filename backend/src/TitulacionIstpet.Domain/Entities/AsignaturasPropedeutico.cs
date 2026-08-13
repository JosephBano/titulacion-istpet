using System;
using System.Collections.Generic;

namespace TitulacionIstpet.Domain.Entities;

public partial class AsignaturasPropedeutico
{
    public int IdAsignatura { get; set; }

    public string? Asignatura { get; set; }

    public bool? Activa { get; set; }
}
