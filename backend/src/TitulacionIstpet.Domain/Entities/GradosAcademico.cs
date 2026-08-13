using System;
using System.Collections.Generic;

namespace TitulacionIstpet.Domain.Entities;

public partial class GradosAcademico
{
    public int IdGradoAcademico { get; set; }

    public int IdNivelAcademico { get; set; }

    public string? Nombre { get; set; }

    public virtual NivelesAcademico IdNivelAcademicoNavigation { get; set; } = null!;

    public virtual ICollection<TitulosEnCurso> TitulosEnCursos { get; set; } = [];

    public virtual ICollection<TitulosProfesore> TitulosProfesores { get; set; } = [];
}
