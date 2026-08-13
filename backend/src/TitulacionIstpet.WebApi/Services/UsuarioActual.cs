using System.Security.Claims;
using TitulacionIstpet.Application.Common.Interfaces;

namespace TitulacionIstpet.WebApi.Services;

public class UsuarioActual(IHttpContextAccessor accessor) : IUsuarioActual
{
    private readonly IHttpContextAccessor _accessor = accessor;

    public string? UserId =>
        _accessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
}
