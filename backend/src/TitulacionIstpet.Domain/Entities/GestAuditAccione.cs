using System;
using System.Collections.Generic;

namespace TitulacionIstpet.Domain.Entities;

public partial class GestAuditAccione
{
    public string IdAuditAcciones { get; set; } = null!;

    public string CodigoSistema { get; set; } = null!;

    public string IdModulo { get; set; } = null!;

    public string? Nombre { get; set; }

    public string? Descripcion { get; set; }

    public bool? EsActivo { get; set; }
}
