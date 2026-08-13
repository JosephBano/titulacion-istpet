using System;
using System.Collections.Generic;

namespace TitulacionIstpet.Domain.Entities;

public partial class SubcategoriaVehiculo
{
    public int IdSubcategoria { get; set; }

    public string? Subcategoria { get; set; }

    public bool? Activa { get; set; }
}
