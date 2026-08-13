using System;
using System.Collections.Generic;

namespace TitulacionIstpet.Domain.Entities;

public partial class Curso
{
    public int IdNivel { get; set; }

    public int IdCarrera { get; set; }

    public string? Nivel { get; set; }

    public int? Jerarquia { get; set; }

    public int? Orden { get; set; }

    public bool? EsRecuperacion { get; set; }

    public string? AliasCurso { get; set; }

    public virtual ICollection<Detallemalla> Detallemallas { get; set; } = [];

    public virtual Carrera IdCarreraNavigation { get; set; } = null!;

    public virtual ICollection<Matricula> Matriculas { get; set; } = [];

    public virtual ICollection<Solicitudescalificacione> Solicitudescalificaciones { get; set; } = [];
}
