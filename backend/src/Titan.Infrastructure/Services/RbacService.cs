using Microsoft.EntityFrameworkCore;
using Titan.Application.DTOs.Auth;
using Titan.Application.Interfaces;
using Titan.Infrastructure.Persistence;

namespace Titan.Infrastructure.Services;

public class RbacService : IRbacService
{
    private readonly TitanDbContext _context;

    public RbacService(TitanDbContext context)
    {
        _context = context;
    }

    public async Task<UserPermissionsDto> BuildUserPermissionsAsync(int idUsuario, string systemCode = "TITAN", CancellationToken cancellationToken = default)
    {
        var user = await _context.usuarios
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.idUsuario == idUsuario && u.activo == 1, cancellationToken);

        if (user == null)
        {
            throw new InvalidOperationException("El usuario especificado no existe o no está activo.");
        }

        // Obtener los roles del usuario que estén activos
        var userRoles = await _context.rbac_usuario_rol
            .AsNoTracking()
            .Include(ur => ur.idRolNavigation)
            .Where(ur => ur.idUsuario == idUsuario && ur.esActivo == 1 && (ur.idRolNavigation.esActivo == 1))
            .Select(ur => ur.idRolNavigation)
            .ToListAsync(cancellationToken);

        var roleIds = userRoles.Select(r => r.idRol).ToList();

        // Obtener todos los permisos asignados filtrados estrictamente por systemCode ("TITAN")
        var permissionsQuery = await _context.rbac_rol_modulo_operacion
            .AsNoTracking()
            .Include(rmo => rmo.idModulosOperacionesNavigation)
                .ThenInclude(mo => mo.idModulosNavigation)
                    .ThenInclude(m => m.id_sistemaNavigation)
            .Include(rmo => rmo.idModulosOperacionesNavigation)
                .ThenInclude(mo => mo.idOperacionesNavigation)
            .Where(rmo => roleIds.Contains(rmo.idRol) && rmo.esActivo == 1 && rmo.idModulosOperacionesNavigation.esActivo == 1)
            .Where(rmo => rmo.idModulosOperacionesNavigation.idModulosNavigation.esActivo == 1)
            .Where(rmo => string.IsNullOrEmpty(systemCode) || rmo.idModulosOperacionesNavigation.idModulosNavigation.id_sistemaNavigation.codigo == systemCode)
            .ToListAsync(cancellationToken);

        var modulesGrouped = permissionsQuery
            .GroupBy(p => p.idModulosOperacionesNavigation.idModulosNavigation)
            .Select(g => new RbacModuloPermissionsDto
            {
                IdModulo = g.Key.idModulos,
                NombreModulo = g.Key.Nombre,
                Operaciones = g.Select(p => p.idModulosOperacionesNavigation.idOperacionesNavigation.NombreOperacion)
                               .Distinct()
                               .ToList()
            })
            .ToList();

        // Normalizar roles asignados excluyendo duplicados y estandarizando al prefijo TITAN_
        var titanRoleIds = permissionsQuery.Select(p => p.idRol).Distinct().ToList();
        var systemRoles = userRoles
            .Where(r => titanRoleIds.Contains(r.idRol) ||
                        (!string.IsNullOrEmpty(systemCode) && r.codigo_rol != null && r.codigo_rol.StartsWith(systemCode, StringComparison.OrdinalIgnoreCase)))
            .Select(r =>
            {
                var code = (r.codigo_rol ?? string.Empty).Trim().ToUpperInvariant();
                return code switch
                {
                    "ADMINISTRADOR" or "ADMIN_SIST" or "TITAN_ADMIN" or "TITAN_ADMINISTRADOR" => "TITAN_ADMINISTRADOR",
                    "DOCENTE" or "PROFESOR" or "TITAN_DOCENTE" => "TITAN_DOCENTE",
                    "ESTUDIANTE" or "ALUMNO" or "TITAN_ESTUDIANTE" => "TITAN_ESTUDIANTE",
                    _ => code.StartsWith("TITAN_") ? code : $"TITAN_{code}"
                };
            })
            .Where(r => !string.IsNullOrWhiteSpace(r))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        return new UserPermissionsDto
        {
            IdUsuario = user.idUsuario,
            Nombre = user.nombre ?? string.Empty,
            EmailInstitucional = user.emailInstitucional ?? string.Empty,
            IdSigafi = user.idSigafi ?? string.Empty,
            TablaSigafi = user.tablaSigafi ?? string.Empty,
            Roles = systemRoles,
            Modulos = modulesGrouped
        };
    }

    public async Task<bool> HasPermissionAsync(int idUsuario, string moduleName, string operationName, CancellationToken cancellationToken = default)
    {
        var permissions = await BuildUserPermissionsAsync(idUsuario, "TITAN", cancellationToken);
        var modulo = permissions.Modulos.FirstOrDefault(m => string.Equals(m.NombreModulo, moduleName, StringComparison.OrdinalIgnoreCase));
        if (modulo == null) return false;

        return modulo.Operaciones.Any(o => string.Equals(o, operationName, StringComparison.OrdinalIgnoreCase));
    }
}
