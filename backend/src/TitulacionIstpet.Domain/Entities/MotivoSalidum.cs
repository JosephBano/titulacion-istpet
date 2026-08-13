using System;
using System.Collections.Generic;

namespace TitulacionIstpet.Domain.Entities;

public partial class MotivoSalidum
{
    public int IdMotivoSalida { get; set; }

    public string? NombreMotivo { get; set; }

    public bool? NecesitaInfrome { get; set; }

    public bool? Esactivo { get; set; }

    public virtual ICollection<ProfesoresMotivoSalidum> ProfesoresMotivoSalida { get; set; } = [];
}
