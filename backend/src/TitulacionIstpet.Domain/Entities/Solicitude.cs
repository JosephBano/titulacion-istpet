using System;
using System.Collections.Generic;

namespace TitulacionIstpet.Domain.Entities;

public partial class Solicitude
{
    public int IdSolicitud { get; set; }

    public int? IdTipoSolicitud { get; set; }

    public string? Cedula { get; set; }

    public string? Solicitante { get; set; }

    public string? Carrera { get; set; }

    public string? Nivel { get; set; }

    public string? Asunto { get; set; }

    public bool? Impreso { get; set; }

    public DateTime? FechaVenta { get; set; }

    public DateTime? FechaImpresion { get; set; }

    public string? CodigoSolicitud { get; set; }

    public bool? Reimprimir { get; set; }

    public bool? Anulada { get; set; }

    public bool? EsAlumno { get; set; }

    public bool? EsDocente { get; set; }

    public bool? EsExterno { get; set; }

    public string? EmailSolicitante { get; set; }

    public bool? EsperandoImpresion { get; set; }

    public bool? RevisarLogs { get; set; }

    public string? IdPeriodo { get; set; }

    public string? UsuarioVenta { get; set; }

    public virtual Tipossolicitude? IdTipoSolicitudNavigation { get; set; }

    public virtual ICollection<Solicitudescalificacione> Solicitudescalificaciones { get; set; } = new List<Solicitudescalificacione>();
}
