using System;
using System.Collections.Generic;

namespace TitulacionIstpet.Domain.Entities;

public partial class AlumnosHabilidade
{
    public int IdalumnosHabilidades { get; set; }

    public string? IdAlumno { get; set; }

    public int Idhabilidades { get; set; }

    public string? Nivel { get; set; }

    public DateTime? FechaCreacion { get; set; }

    public DateTime? FechaModificacion { get; set; }

    public virtual Habilidade IdhabilidadesNavigation { get; set; } = null!;
}
