using System;
using System.Collections.Generic;

namespace TitulacionIstpet.Domain.Entities;

public partial class BienUsuarioCaso
{
    public int IdUsuarioCaso { get; set; }

    public int IdTipoSujeto { get; set; }

    public int IdUsuario { get; set; }

    public int IdCaso { get; set; }

    public string Rol { get; set; } = null!;

    public string? Detalle { get; set; }

    public virtual ICollection<BienAsistentesDesarrollo> BienAsistentesDesarrollos { get; set; } = new List<BienAsistentesDesarrollo>();

    public virtual BienCaso IdCasoNavigation { get; set; } = null!;

    public virtual BienTipoSujeto IdTipoSujetoNavigation { get; set; } = null!;

    public virtual Usuario IdUsuarioNavigation { get; set; } = null!;
}
