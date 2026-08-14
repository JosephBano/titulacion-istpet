using System;
using System.Collections.Generic;

namespace TitulacionIstpet.Domain.Entities;

public partial class AsignacionesProfesore
{
    public string IdProfesor { get; set; } = null!;

    public int IdAsignatura { get; set; }

    public string IdPeriodo { get; set; } = null!;

    public int IdModalidad { get; set; }

    public int IdSeccion { get; set; }

    public int IdNivel { get; set; }

    public string Paralelo { get; set; } = null!;

    public bool? Activo { get; set; }

    public DateTime? FechaGrabar { get; set; }

    public DateTime? FechaModificacion { get; set; }

    public string? CodigoAsignacion { get; set; }

    public bool? EntregaActa { get; set; }

    public bool? IngresaNotas { get; set; }

    public string? UserAsignaciones { get; set; }

    public DateOnly? FechaFin { get; set; }

    public DateOnly? FechaInicial { get; set; }

    public string? UserActa { get; set; }

    public int IdAsignacion { get; set; }

    public bool? EsActivaAsignacion { get; set; }

    public decimal? NumeroHoras { get; set; }

    public bool? ContabilizarHoraDocente { get; set; }

    public decimal? HorasPracticoExperimental { get; set; }

    public bool? ExtraCurricular { get; set; }

    public virtual ICollection<CondAcadSesione> CondAcadSesiones { get; set; } = [];

    public virtual ICollection<HorarioDetalle> HorarioDetalles { get; set; } = [];
}
