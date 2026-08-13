using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Titan.Api.Attributes;
using Titan.Application.Interfaces;

namespace Titan.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class RbacController : ControllerBase
{
    private readonly IRbacManagementService _rbacManagementService;

    public RbacController(IRbacManagementService rbacManagementService)
    {
        _rbacManagementService = rbacManagementService;
    }

    /// <summary>
    /// Obtener todos los sistemas registrados en la base de datos (RBAC)
    /// </summary>
    [HttpGet("sistemas")]
    public async Task<IActionResult> GetSistemas(CancellationToken cancellationToken)
    {
        var sistemas = await _rbacManagementService.GetSistemasAsync(cancellationToken);
        return Ok(sistemas);
    }

    /// <summary>
    /// Obtener módulos y sus operaciones asociadas filtrando por sistema
    /// </summary>
    [HttpGet("sistemas/{idSistema:int}/modulos")]
    public async Task<IActionResult> GetModulosBySistema(int idSistema, CancellationToken cancellationToken)
    {
        var modulos = await _rbacManagementService.GetModulosBySistemaAsync(idSistema, cancellationToken);
        return Ok(modulos);
    }

    /// <summary>
    /// Listar los roles activos en el sistema
    /// </summary>
    [HttpGet("roles")]
    public async Task<IActionResult> GetRoles(CancellationToken cancellationToken)
    {
        var roles = await _rbacManagementService.GetRolesAsync(cancellationToken);
        return Ok(roles);
    }

    /// <summary>
    /// Crear un nuevo rol (requiere permiso de Administración RBAC)
    /// </summary>
    [HttpPost("roles")]
    [HasPermission("SEGURIDAD_RBAC", "CREAR_ROL")]
    public async Task<IActionResult> CreateRol([FromBody] CreateRolRequest request, CancellationToken cancellationToken)
    {
        var rol = await _rbacManagementService.CreateRolAsync(request.Nombre, request.CodigoRol, cancellationToken);
        return CreatedAtAction(nameof(GetRoles), new { id = rol.idRol }, rol);
    }

    /// <summary>
    /// Asignar un rol a un usuario
    /// </summary>
    [HttpPost("usuarios/{idUsuario:int}/roles/{idRol:int}")]
    [HasPermission("SEGURIDAD_RBAC", "ASIGNAR_ROL")]
    public async Task<IActionResult> AssignRolToUsuario(int idUsuario, int idRol, CancellationToken cancellationToken)
    {
        var result = await _rbacManagementService.AssignRolToUsuarioAsync(idUsuario, idRol, cancellationToken);
        return Ok(new { success = result, message = "Rol asignado correctamente." });
    }

    /// <summary>
    /// Revocar un rol a un usuario
    /// </summary>
    [HttpDelete("usuarios/{idUsuario:int}/roles/{idRol:int}")]
    [HasPermission("SEGURIDAD_RBAC", "DESASIGNAR_ROL")]
    public async Task<IActionResult> RemoveRolFromUsuario(int idUsuario, int idRol, CancellationToken cancellationToken)
    {
        var result = await _rbacManagementService.RemoveRolFromUsuarioAsync(idUsuario, idRol, cancellationToken);
        return Ok(new { success = result, message = "Rol removido correctamente." });
    }

    /// <summary>
    /// Asignar una operación de un módulo a un rol (matriz de permisos)
    /// </summary>
    [HttpPost("roles/{idRol:int}/permisos/{idModuloOperacion:int}")]
    [HasPermission("SEGURIDAD_RBAC", "CONFIGURAR_PERMISOS")]
    public async Task<IActionResult> AssignPermissionToRol(int idRol, int idModuloOperacion, CancellationToken cancellationToken)
    {
        var result = await _rbacManagementService.AssignPermissionToRolAsync(idRol, idModuloOperacion, cancellationToken);
        return Ok(new { success = result, message = "Permiso asignado al rol exitosamente." });
    }
}

public record CreateRolRequest(string Nombre, string CodigoRol);
