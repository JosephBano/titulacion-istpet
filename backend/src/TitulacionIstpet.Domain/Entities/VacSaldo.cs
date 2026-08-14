using System;
using System.Collections.Generic;

namespace TitulacionIstpet.Domain.Entities;

public partial class VacSaldo
{
    public int IdSaldoVacaciones { get; set; }

    public string IdProfesor { get; set; } = null!;

    public string Periodo { get; set; } = null!;

    public decimal DiasGanados { get; set; }

    public decimal DiasTomados { get; set; }

    public decimal DiasAcumulados { get; set; }

    public int FinesSemanaTomados { get; set; }

    public DateOnly FechaUltimoCalculo { get; set; }

    public bool? Activo { get; set; }
}
