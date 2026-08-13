using System;
using System.Collections.Generic;

namespace TitulacionIstpet.Domain.Entities;

public partial class PlantillaContrato
{
    public int IdPlantillaContrato { get; set; }

    public int IdDedicacion { get; set; }

    public int IdTiposContratos { get; set; }

    public int IdInstitucionesInstituto { get; set; }

    public int IdSello { get; set; }

    public int IdFondo { get; set; }

    public string? Titulo { get; set; }

    public string? Cuerpo { get; set; }

    public int? Version { get; set; }

    public DateOnly? FechaCreacion { get; set; }

    public DateOnly? FechaModificacion { get; set; }

    public bool? EsActivo { get; set; }

    public bool? EsDocente { get; set; }

    public virtual Dedicacion IdDedicacionNavigation { get; set; } = null!;

    public virtual AdjuntosImagene IdFondoNavigation { get; set; } = null!;

    public virtual InstitucionesInstituto IdInstitucionesInstitutoNavigation { get; set; } = null!;

    public virtual AdjuntosImagene IdSelloNavigation { get; set; } = null!;

    public virtual TiposContrato IdTiposContratosNavigation { get; set; } = null!;

    public virtual ICollection<PlantillaClausula> PlantillaClausulas { get; set; } = new List<PlantillaClausula>();
}
