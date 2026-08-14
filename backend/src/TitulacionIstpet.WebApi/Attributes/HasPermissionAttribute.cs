using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace TitulacionIstpet.WebApi.Attributes;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = true)]
public class HasPermissionAttribute : AuthorizeAttribute, IAuthorizationFilter
{
    private readonly string _permission;

    public HasPermissionAttribute(string module, string operation)
    {
        _permission = $"{module}:{operation}";
    }

    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var user = context.HttpContext.User;
        if (!user.Identity?.IsAuthenticated ?? true)
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        var hasPermission = user.Claims.Any(c => c.Type == "permission" && string.Equals(c.Value, _permission, StringComparison.OrdinalIgnoreCase));
        if (!hasPermission)
        {
            context.Result = new ForbidResult();
        }
    }
}
