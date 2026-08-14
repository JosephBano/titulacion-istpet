using System;
using System.Collections.Generic;

namespace TitulacionIstpet.Domain.Entities;

public partial class Vinculacionobjetivospedi
{
    public int IdObjetivoPedi { get; set; }

    public string? Pedi { get; set; }

    public string? ObjetivoPedi { get; set; }

    public bool? Activo { get; set; }

    public virtual ICollection<Vinculacionproyectosobjetivospedi> Vinculacionproyectosobjetivospedis { get; set; } = [];
}
