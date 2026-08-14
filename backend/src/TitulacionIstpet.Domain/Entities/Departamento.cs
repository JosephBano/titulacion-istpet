using System;
using System.Collections.Generic;

namespace TitulacionIstpet.Domain.Entities;

public partial class Departamento
{
    public int Iddepartamentos { get; set; }

    public string? NombreDepartamento { get; set; }

    public string? Abreviacion { get; set; }

    public string? Descripcion { get; set; }

    public int? IdInstitucion { get; set; }

    public virtual ICollection<OfertasLaborale> OfertasLaborales { get; set; } = [];
}
