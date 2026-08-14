using System;
using System.Collections.Generic;

namespace TitulacionIstpet.Domain.Entities;

public partial class GestAuditRegistro
{
    public long IdAuditRegistros { get; set; }

    public DateTime FechaHora { get; set; }

    public string CodigoSistema { get; set; } = null!;

    public string IdUsuario { get; set; } = null!;

    public string? Rol { get; set; }

    public string IdModulo { get; set; } = null!;

    public string Accion { get; set; } = null!;

    public int? IdEntidad { get; set; }

    public string? TablaAfectada { get; set; }

    public string? Descripcion { get; set; }

    public string? DatosAnteriores { get; set; }

    public string? DatosNuevos { get; set; }

    public string? IpOrigen { get; set; }

    public string? UserAgent { get; set; }

    public string? Jti { get; set; }

    public string? RequestMethod { get; set; }

    public string? RequestPath { get; set; }

    public int? StatusCode { get; set; }

    public string? MensajeError { get; set; }

    public int? DuracionMs { get; set; }
}
