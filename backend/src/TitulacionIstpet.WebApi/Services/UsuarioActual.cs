using System.Security.Claims;
using TitulacionIstpet.Application.Common.Interfaces;

namespace TitulacionIstpet.WebApi.Services;

public class UsuarioActual : IUsuarioActual
{
    private readonly IHttpContextAccessor _accessor;

    public UsuarioActual(IHttpContextAccessor accessor) => _accessor = accessor;

    public string? UserId =>
        _accessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
}
