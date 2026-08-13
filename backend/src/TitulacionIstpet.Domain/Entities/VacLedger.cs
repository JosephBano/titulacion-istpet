using System;
using System.Collections.Generic;

namespace TitulacionIstpet.Domain.Entities;

public partial class VacLedger
{
    public int IdLedger { get; set; }

    public string IdProfesor { get; set; } = null!;

    public string TipoTransaccion { get; set; } = null!;

    public decimal Dias { get; set; }

    public int FinesSemana { get; set; }

    public DateTime Fecha { get; set; }

    public string Periodo { get; set; } = null!;

    public string Detalle { get; set; } = null!;

    public int? IdPeriodoVacaciones { get; set; }

    public int? IdPermiso { get; set; }

    public int? RegistradoPorId { get; set; }
}
