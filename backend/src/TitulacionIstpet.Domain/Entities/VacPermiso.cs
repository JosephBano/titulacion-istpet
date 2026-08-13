using System;
using System.Collections.Generic;

namespace TitulacionIstpet.Domain.Entities;

public partial class VacPermiso
{
    public int IdPermiso { get; set; }

    public string IdProfesor { get; set; } = null!;

    public int IdTipoPermiso { get; set; }

    public decimal? HorasSolicitadas { get; set; }

    public int? DiasSolicitados { get; set; }

    public string Estado { get; set; } = null!;

    public DateOnly FechaSuceso { get; set; }

    public DateTime FechaSolicitud { get; set; }

    public string Motivo { get; set; } = null!;

    public string? RutaJustificativo { get; set; }

    public DateTime? FechaEntregaJustificativo { get; set; }

    public int? AprobadoPorId { get; set; }

    public string? NotasRrhh { get; set; }

    public bool AfectaVacaciones { get; set; }

    public bool AdjuntoPendienteFisico { get; set; }

    public virtual ICollection<VacRecuperacionTiempo> VacRecuperacionTiempos { get; set; } = [];
}
