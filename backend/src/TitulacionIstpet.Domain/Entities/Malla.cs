using System;
using System.Collections.Generic;

namespace TitulacionIstpet.Domain.Entities;

public partial class Malla
{
    public int IdMalla { get; set; }

    public int IdCarrera { get; set; }

    public int? Vigencia { get; set; }

    public string? Descripcion { get; set; }

    public int? CreditosMinimo { get; set; }

    public int? CreditosMaximo { get; set; }

    public int? CreditosReprobatorio { get; set; }

    public bool? Activa { get; set; }

    public virtual ICollection<Cambiosmalla> Cambiosmallas { get; set; } = [];

    public virtual ICollection<Detallemalla> Detallemallas { get; set; } = [];

    public virtual Carrera IdCarreraNavigation { get; set; } = null!;
}
