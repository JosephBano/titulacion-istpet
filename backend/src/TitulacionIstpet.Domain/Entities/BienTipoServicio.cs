using System;
using System.Collections.Generic;

namespace TitulacionIstpet.Domain.Entities;

public partial class BienTipoServicio
{
    public int IdTipoServicio { get; set; }

    public string Nombre { get; set; } = null!;

    public bool EsActivo { get; set; }

    public virtual ICollection<BienServiciosFicha> BienServiciosFichas { get; set; } = [];
}
