using System;
using System.Collections.Generic;

namespace TitulacionIstpet.Domain.Entities;

public partial class Usuariosdepartamentossolicitude
{
    public int IdUsuarioDepartamentoSolicitud { get; set; }

    public int? IdTipoSolicitud { get; set; }

    public int? IdUsuarioSolicitud { get; set; }

    public bool? Activo { get; set; }

    public DateTime? FechaRegistro { get; set; }

    public DateTime? FechaBaja { get; set; }

    public virtual Tipossolicitude? IdTipoSolicitudNavigation { get; set; }

    public virtual Usuariossolicitude? IdUsuarioSolicitudNavigation { get; set; }
}
