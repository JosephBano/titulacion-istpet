using System;
using System.Collections.Generic;

namespace TitulacionIstpet.Domain.Entities;

public partial class Configsharepoint
{
    public int IdSharePoint { get; set; }

    public string ClientId { get; set; } = null!;

    public string TenanId { get; set; } = null!;

    public string ClientSecret { get; set; } = null!;

    public string AppId { get; set; } = null!;

    public string RedirectUrl { get; set; } = null!;

    public string TenantName { get; set; } = null!;

    public string SiteName { get; set; } = null!;

    public string SiteId { get; set; } = null!;

    public string ListId { get; set; } = null!;

    public string DriveId { get; set; } = null!;

    public bool? EsActivo { get; set; }

    public DateOnly? FechaCreado { get; set; }

    public DateOnly? FechaActualizado { get; set; }

    public string? Correo { get; set; }

    public string? Password { get; set; }
}
