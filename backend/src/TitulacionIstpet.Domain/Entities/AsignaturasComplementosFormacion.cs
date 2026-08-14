using System;
using System.Collections.Generic;

namespace TitulacionIstpet.Domain.Entities;

public partial class AsignaturasComplementosFormacion
{
    public int IdAsignatura { get; set; }

    public int? IdCarrera { get; set; }

    public string? Asignatura { get; set; }

    public bool? Activo { get; set; }
}
