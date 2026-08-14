using System;
using System.Collections.Generic;

namespace TitulacionIstpet.Domain.Entities;

public partial class FechasPago
{
    public int IdFecha { get; set; }

    public int? IdEspecie { get; set; }

    public DateOnly? Fecha { get; set; }
}
