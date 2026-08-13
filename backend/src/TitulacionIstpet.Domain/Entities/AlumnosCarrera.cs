using System;
using System.Collections.Generic;

namespace TitulacionIstpet.Domain.Entities;

public partial class AlumnosCarrera
{
    public string IdAlumno { get; set; } = null!;

    public int IdCarrera { get; set; }

    public bool? Convalidacion { get; set; }

    public string? CarreraConvalidada { get; set; }

    public string? InstitucionConvalidada { get; set; }

    public int? CreditosConvalidados { get; set; }

    public bool? Pasantias { get; set; }

    public decimal? NotaPasantia { get; set; }

    public int? CreditosPasantia { get; set; }

    public bool? TrabajoGrado { get; set; }

    public decimal? NotaDocumento { get; set; }

    public decimal? NotaDefensa { get; set; }

    public decimal? NotaTesis { get; set; }

    public int? CreditosTitulo { get; set; }
}
