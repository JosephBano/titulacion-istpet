using System;
using System.Collections.Generic;

namespace TitulacionIstpet.Domain.Entities;

public partial class DocumentosAdjunto
{
    public int IddocumentosAdjuntos { get; set; }

    public string IdAlumno { get; set; } = null!;

    public int IdtiposDocumentos { get; set; }

    public string? NombreArchivo { get; set; }

    public string? RutaArchivo { get; set; }

    public DateTime? FechaSubida { get; set; }

    public DateTime? FechaModificacion { get; set; }

    public virtual TiposDocumento IdtiposDocumentosNavigation { get; set; } = null!;

    public virtual ICollection<Postulacione> Postulaciones { get; set; } = new List<Postulacione>();
}
