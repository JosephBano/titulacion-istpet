using System;
using System.Collections.Generic;

namespace TitulacionIstpet.Domain.Entities;

public partial class BienTribunal
{
    public int IdUsuario { get; set; }

    public bool EsActivo { get; set; }

    public DateTime FechaCreacion { get; set; }

    public DateTime? FechaActualizacion { get; set; }

    public int? IdTipoCargoTribunal { get; set; }

    public virtual ICollection<BienVotosTribunale> BienVotosTribunales { get; set; } = [];

    public virtual BienTipoCargoTribunal? IdTipoCargoTribunalNavigation { get; set; }

    public virtual Usuario IdUsuarioNavigation { get; set; } = null!;
}
