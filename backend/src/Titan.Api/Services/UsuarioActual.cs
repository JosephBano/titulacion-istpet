using System.Security.Claims;
using Titan.Application.Common.Interfaces;

namespace Titan.Api.Services;

public class UsuarioActual : IUsuarioActual
{
    private readonly IHttpContextAccessor _accessor;

    public UsuarioActual(IHttpContextAccessor accessor) => _accessor = accessor;

    public string? UserId =>
        _accessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
}
