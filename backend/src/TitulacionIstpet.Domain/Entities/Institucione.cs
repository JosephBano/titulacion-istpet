using System;
using System.Collections.Generic;

namespace TitulacionIstpet.Domain.Entities;

public partial class Institucione
{
    public int IdInstitucion { get; set; }

    public string? Institucion { get; set; }

    public string? Ciudad { get; set; }

    public string? Provincia { get; set; }
}
