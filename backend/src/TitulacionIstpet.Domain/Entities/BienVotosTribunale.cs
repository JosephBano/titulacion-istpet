using System;
using System.Collections.Generic;

namespace TitulacionIstpet.Domain.Entities;

public partial class BienVotosTribunale
{
    public int IdVotosTribunales { get; set; }

    public string Estado { get; set; } = null!;

    public DateTime FechaCreacion { get; set; }

    public int IdResolucionesTribunales { get; set; }

    public int IdUsuarioTribunal { get; set; }

    public string? Observaciones { get; set; }

    public int? IdCargoOcupado { get; set; }

    public virtual BienTipoCargoTribunal? IdCargoOcupadoNavigation { get; set; }

    public virtual BienResolucionesTribunale IdResolucionesTribunalesNavigation { get; set; } = null!;

    public virtual BienTribunal IdUsuarioTribunalNavigation { get; set; } = null!;
}
