using System;
using System.Collections.Generic;

namespace TitulacionIstpet.Domain.Entities;

public partial class Parametrostipossolicitude
{
    public int IdParametroTipoSolicitud { get; set; }

    public int? IdTipoSolicitud { get; set; }

    public bool? Periodo { get; set; }

    public bool? EsPeriodoApertura { get; set; }

    public bool? EsConduccion { get; set; }

    public bool? Carrera { get; set; }

    public bool? Nivel { get; set; }

    public bool? Asignatura { get; set; }

    public bool? Detalle { get; set; }

    public bool? EsDetalleAutogenerado { get; set; }

    public string? DetalleAutogenerado { get; set; }

    public bool? Activo { get; set; }

    public bool? EsCalificaciones { get; set; }

    public virtual Tipossolicitude? IdTipoSolicitudNavigation { get; set; }
}
