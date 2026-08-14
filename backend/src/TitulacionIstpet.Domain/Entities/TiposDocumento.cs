using System;
using System.Collections.Generic;

namespace TitulacionIstpet.Domain.Entities;

public partial class TiposDocumento
{
    public int IdtiposDocumentos { get; set; }

    public string? Documento { get; set; }

    public string? SubijoDocumento { get; set; }

    public virtual ICollection<DocumentosAdjunto> DocumentosAdjuntos { get; set; } = [];
}
