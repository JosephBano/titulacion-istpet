using System;
using System.Collections.Generic;

namespace TitulacionIstpet.Domain.Entities;

public partial class Usuariossolicitude
{
    public int IdUsuarioSolicitud { get; set; }

    public string? Usuario { get; set; }

    public string? Clave { get; set; }

    public bool? Resetear { get; set; }

    public string? Email { get; set; }

    public bool? Activo { get; set; }

    public bool? Administrador { get; set; }

    public string? Nombre { get; set; }

    public virtual ICollection<Usuariosdepartamentossolicitude> Usuariosdepartamentossolicitudes { get; set; } = new List<Usuariosdepartamentossolicitude>();
}
