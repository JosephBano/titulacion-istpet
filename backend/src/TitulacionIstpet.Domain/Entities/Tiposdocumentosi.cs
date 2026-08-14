using System;
using System.Collections.Generic;

namespace TitulacionIstpet.Domain.Entities;

public partial class Tiposdocumentosi
{
    public string TipoDocumento { get; set; } = null!;

    public string? Documento { get; set; }

    public bool? Activo { get; set; }
}
