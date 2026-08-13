using System;
using System.Collections.Generic;

namespace TitulacionIstpet.Domain.Entities;

public partial class RbacRefreshToken
{
    public ulong IdRefreshToken { get; set; }

    public int IdUsuario { get; set; }

    public string TokenHash { get; set; } = null!;

    public string? DeviceInfo { get; set; }

    public string? IpAddress { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime ExpiresAt { get; set; }

    public DateTime? RevokedAt { get; set; }

    public ulong? ReplacedByTokenId { get; set; }

    public string? FamilyId { get; set; }

    public uint? Sequence { get; set; }

    public string? RevokedReason { get; set; }

    public virtual Usuario IdUsuarioNavigation { get; set; } = null!;
}
