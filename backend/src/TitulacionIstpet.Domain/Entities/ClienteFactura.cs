using System;
using System.Collections.Generic;

namespace TitulacionIstpet.Domain.Entities;

public partial class ClienteFactura
{
    public string DocumentoFactura { get; set; } = null!;

    public string? TipoDocumento { get; set; }

    public string? Nombre { get; set; }

    public string? Direccion { get; set; }

    public string? Telefono { get; set; }

    public string? Email { get; set; }

    public DateTime? FechaCreacion { get; set; }
}
