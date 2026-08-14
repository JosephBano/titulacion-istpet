using System;
using System.Collections.Generic;

namespace TitulacionIstpet.Domain.Entities;

public partial class HorarioDetalle
{
    public int IdHorario { get; set; }

    public int IdAsignacion { get; set; }

    public int IdFecha { get; set; }

    public int Idhora { get; set; }

    public int? IdEspacio { get; set; }

    public string? TipoBloque { get; set; }

    public bool? Activo { get; set; }

    public bool? ClaseReasignacion { get; set; }

    public bool? EsRecuperacionPedagocia { get; set; }

    public string? Observacion { get; set; }

    public int? IdHorarioReasgincacion { get; set; }

    public virtual AsignacionesProfesore IdAsignacionNavigation { get; set; } = null!;

    public virtual Espacio? IdEspacioNavigation { get; set; }

    public virtual FechasHorario IdFechaNavigation { get; set; } = null!;

    public virtual HorasClase IdhoraNavigation { get; set; } = null!;
}
