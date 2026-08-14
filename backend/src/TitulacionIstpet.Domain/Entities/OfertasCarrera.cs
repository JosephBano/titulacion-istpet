using System;
using System.Collections.Generic;

namespace TitulacionIstpet.Domain.Entities;

public partial class OfertasCarrera
{
    public int IdofertasCarreras { get; set; }

    public int IdofertasLaborales { get; set; }

    public int IdCarrera { get; set; }

    public virtual Carrera IdCarreraNavigation { get; set; } = null!;

    public virtual OfertasLaborale IdofertasLaboralesNavigation { get; set; } = null!;
}
