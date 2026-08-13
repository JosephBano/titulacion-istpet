using System;
using System.Collections.Generic;

namespace TitulacionIstpet.Domain.Entities;

public partial class HabilidadesRequerida
{
    public int IdhabilidadesRequeridas { get; set; }

    public int IdofertasLaborales { get; set; }

    public int Idhabilidades { get; set; }

    public string? Nivel { get; set; }

    public bool? EsObligatoria { get; set; }

    public virtual Habilidade IdhabilidadesNavigation { get; set; } = null!;

    public virtual OfertasLaborale IdofertasLaboralesNavigation { get; set; } = null!;
}
