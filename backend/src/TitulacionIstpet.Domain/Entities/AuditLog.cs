using System;
using System.Collections.Generic;

namespace TitulacionIstpet.Domain.Entities;

public partial class AuditLog
{
    public int Id { get; set; }

    public string Usuario { get; set; } = null!;

    public string Accion { get; set; } = null!;

    public string? EntidadId { get; set; }

    public string? Detalles { get; set; }

    public string? IpOrigen { get; set; }

    public DateTime FechaHora { get; set; }
}
