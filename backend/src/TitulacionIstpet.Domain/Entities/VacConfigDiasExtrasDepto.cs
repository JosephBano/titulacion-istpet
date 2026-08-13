using System;
using System.Collections.Generic;

namespace TitulacionIstpet.Domain.Entities;

public partial class VacConfigDiasExtrasDepto
{
    public int IdConfig { get; set; }

    public int Iddepartamentos { get; set; }

    public int? IdInstitucion { get; set; }

    public decimal DiasExtras { get; set; }

    public bool RequiereFinSemana { get; set; }

    public int? CantFinesSemanaRequeridos { get; set; }

    public string? PeriodoAplicacion { get; set; }

    public bool? Activo { get; set; }

    public string? Motivo { get; set; }

    public DateOnly? FechaVigenciaDesde { get; set; }

    public DateOnly? FechaVigenciaHasta { get; set; }

    public int? RegistradoPorId { get; set; }

    public DateTime FechaCreacion { get; set; }

    public virtual ICollection<VacConfigDiasExtrasExcepcione> VacConfigDiasExtrasExcepciones { get; set; } = [];
}
