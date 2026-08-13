using System;
using System.Collections.Generic;

namespace TitulacionIstpet.Domain.Entities;

public partial class Vinculacioncategoriasobjetivosoportunidade
{
    public int IdCategoriaObjetivoOportunidad { get; set; }

    public string? CategoriaObjetivoOportunidad { get; set; }

    public bool? Activo { get; set; }

    public virtual ICollection<Vinculacionobjetivosoportunidade> Vinculacionobjetivosoportunidades { get; set; } = new List<Vinculacionobjetivosoportunidade>();
}
