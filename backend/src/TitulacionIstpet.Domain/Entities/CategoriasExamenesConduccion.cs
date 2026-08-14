using System;
using System.Collections.Generic;

namespace TitulacionIstpet.Domain.Entities;

public partial class CategoriasExamenesConduccion
{
    public int IdCategoria { get; set; }

    public string? Categoria { get; set; }

    public bool? TieneNota { get; set; }

    public bool? Activa { get; set; }
}
