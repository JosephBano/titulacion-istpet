using System;
using System.Collections.Generic;

namespace TitulacionIstpet.Domain.Entities;

public partial class CategoriasTerminosCondicione
{
    public int IdCategoria { get; set; }

    public string? Categoria { get; set; }

    public bool? EsAlumno { get; set; }

    public bool? EsDocente { get; set; }

    public bool? EsAdministrativo { get; set; }

    public bool? EsExterno { get; set; }

    public DateOnly? FechaRegistro { get; set; }

    public bool? Activo { get; set; }
}
