using System;
using System.Collections.Generic;

namespace TitulacionIstpet.Domain.Entities;

public partial class Tipossolicitude
{
    public int IdTipoSolicitud { get; set; }

    public int? IdCategoriaSolicitud { get; set; }

    public int? IdDepartamentoSolicitud { get; set; }

    public string? TipoSolicitud { get; set; }

    public bool? Activo { get; set; }

    public bool? EscuelaConduccion { get; set; }

    public virtual Categoriassolicitude? IdCategoriaSolicitudNavigation { get; set; }

    public virtual Departamentossolicitude? IdDepartamentoSolicitudNavigation { get; set; }

    public virtual ICollection<Parametrostipossolicitude> Parametrostipossolicitudes { get; set; } = new List<Parametrostipossolicitude>();

    public virtual ICollection<Solicitude> Solicitudes { get; set; } = new List<Solicitude>();

    public virtual ICollection<Usuariosdepartamentossolicitude> Usuariosdepartamentossolicitudes { get; set; } = new List<Usuariosdepartamentossolicitude>();
}
