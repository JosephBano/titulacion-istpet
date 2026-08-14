using System;
using System.Collections.Generic;

namespace TitulacionIstpet.Domain.Entities;

public partial class Habilidade
{
    public int Idhabilidades { get; set; }

    public string? Nombre { get; set; }

    public string? Descripcion { get; set; }

    public virtual ICollection<AlumnosHabilidade> AlumnosHabilidades { get; set; } = [];

    public virtual ICollection<HabilidadesRequerida> HabilidadesRequerida { get; set; } = [];
}
