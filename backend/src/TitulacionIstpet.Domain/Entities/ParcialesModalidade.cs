using System;
using System.Collections.Generic;

namespace TitulacionIstpet.Domain.Entities;

public partial class ParcialesModalidade
{
    public int? IdParcial { get; set; }

    public int? IdModalidad { get; set; }

    public bool? Activo { get; set; }
}
