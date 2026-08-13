using System;
using System.Collections.Generic;

namespace TitulacionIstpet.Domain.Entities;

public partial class TipoFuncionario
{
    public int IdTipoFuncionario { get; set; }

    public string? Nombre { get; set; }

    public ulong? EsDocente { get; set; }

    public virtual ICollection<CargoInstituto> CargoInstitutos { get; set; } = new List<CargoInstituto>();
}
