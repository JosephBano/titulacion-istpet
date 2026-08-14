using System;
using System.Collections.Generic;

namespace TitulacionIstpet.Domain.Entities;

public partial class CreditoAlumnoDetalle
{
    public int IdCreditoAlumnoDetalle { get; set; }

    public int? IdCredito { get; set; }

    public DateOnly? FechaPago { get; set; }

    public decimal? ValorCuota { get; set; }

    public decimal? ValorAbonado { get; set; }

    public bool? Cancelado { get; set; }
}
