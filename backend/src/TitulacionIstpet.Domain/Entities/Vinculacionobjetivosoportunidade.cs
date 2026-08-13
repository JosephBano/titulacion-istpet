using System;
using System.Collections.Generic;

namespace TitulacionIstpet.Domain.Entities;

public partial class Vinculacionobjetivosoportunidade
{
    public int IdObjetivoOportunidad { get; set; }

    public int? IdCategoriaObjetivoOportunidad { get; set; }

    public string? ObjetivoOportunidad { get; set; }

    public bool? Activo { get; set; }

    public virtual Vinculacioncategoriasobjetivosoportunidade? IdCategoriaObjetivoOportunidadNavigation { get; set; }

    public virtual ICollection<Vinculacionproyectosobjetivosoportunidade> Vinculacionproyectosobjetivosoportunidades { get; set; } = [];
}
