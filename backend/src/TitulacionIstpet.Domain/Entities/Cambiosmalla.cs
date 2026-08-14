using System;
using System.Collections.Generic;

namespace TitulacionIstpet.Domain.Entities;

public partial class Cambiosmalla
{
    public int IdCambioMalla { get; set; }

    public int IdMalla { get; set; }

    public DateOnly? Fecha { get; set; }

    public string? Cambio { get; set; }

    public virtual Malla IdMallaNavigation { get; set; } = null!;
}
