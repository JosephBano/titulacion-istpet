using System;
using System.Collections.Generic;

namespace TitulacionIstpet.Domain.Entities;

public partial class Universidade
{
    public int IdUniversidad { get; set; }

    public int Idpaises { get; set; }

    public string? Nombre { get; set; }

    public string? CodigoSiees { get; set; }

    public virtual Paise IdpaisesNavigation { get; set; } = null!;

    public virtual ICollection<TitulosEnCurso> TitulosEnCursos { get; set; } = new List<TitulosEnCurso>();

    public virtual ICollection<TitulosProfesore> TitulosProfesores { get; set; } = new List<TitulosProfesore>();
}
