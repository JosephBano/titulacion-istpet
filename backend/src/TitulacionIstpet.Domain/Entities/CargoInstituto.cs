using System;
using System.Collections.Generic;

namespace TitulacionIstpet.Domain.Entities;

public partial class CargoInstituto
{
    public int IdCargoInstituto { get; set; }

    public int IdTipoFuncionario { get; set; }

    public string? Nombre { get; set; }

    public int? DisponibilidadCargo { get; set; }

    public virtual TipoFuncionario IdTipoFuncionarioNavigation { get; set; } = null!;
}
