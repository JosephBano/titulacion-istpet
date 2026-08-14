using Microsoft.EntityFrameworkCore;
using TitulacionIstpet.Application.DTOs.Auth;
using TitulacionIstpet.Application.Interfaces;
using TitulacionIstpet.Infrastructure.Persistence;

namespace TitulacionIstpet.Infrastructure.Services;

public class RbacService : IRbacService
{
    private readonly SigafiDbContext _context;

    public RbacService(SigafiDbContext context)
    {
        _context = context;
    }

    public async Task<UserPermissionsDto> BuildUserPermissionsAsync(int idUsuario, string systemCode = "TITULACION", CancellationToken cancellationToken = default)
    {
        var user = await _context.Usuarios
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.IdUsuario == idUsuario, cancellationToken);

        if (user == null || !user.Activo)
        {
            throw new UnauthorizedAccessException("Usuario inexistente o inactivo.");
        }

        // Obtener todos los roles activos asignados al usuario
        var userRoles = await _context.RbacUsuarioRols
            .AsNoTracking()
            .Include(ur => ur.IdRolNavigation)
            .Where(ur => ur.IdUsuario == idUsuario && ur.EsActivo == true && ur.IdRolNavigation.EsActivo == true)
            .Select(ur => ur.IdRolNavigation)
            .ToListAsync(cancellationToken);

        var roleIds = userRoles.Select(r => r.IdRol).ToList();

        // Obtener todos los permisos asignados filtrados por systemCode
        var permissionsQuery = await _context.RbacRolModuloOperacions
            .AsNoTracking()
            .Include(rmo => rmo.IdModulosOperacionesNavigation)
                .ThenInclude(mo => mo.IdModulosNavigation)
                    .ThenInclude(m => m.IdSistemaNavigation)
            .Include(rmo => rmo.IdModulosOperacionesNavigation)
                .ThenInclude(mo => mo.IdOperacionesNavigation)
            .Where(rmo => roleIds.Contains(rmo.IdRol) && rmo.EsActivo == true && rmo.IdModulosOperacionesNavigation.EsActivo == true)
            .Where(rmo => rmo.IdModulosOperacionesNavigation.IdModulosNavigation.EsActivo == true)
            .Where(rmo => string.IsNullOrEmpty(systemCode) ||
                          rmo.IdModulosOperacionesNavigation.IdModulosNavigation.IdSistemaNavigation.Codigo == systemCode)
            .ToListAsync(cancellationToken);

        var modulesGrouped = permissionsQuery
            .GroupBy(p => p.IdModulosOperacionesNavigation.IdModulosNavigation)
            .Select(g => new RbacModuloPermissionsDto
            {
                IdModulo = g.Key.IdModulos,
                NombreModulo = g.Key.Nombre ?? string.Empty,
                Operaciones = g.Select(p => p.IdModulosOperacionesNavigation.IdOperacionesNavigation.NombreOperacion ?? string.Empty)
                               .Where(o => !string.IsNullOrWhiteSpace(o))
                               .Distinct()
                               .ToList()
            })
            .ToList();

        // Normalizar roles asignados al sistema de Titulación
        var systemRoles = userRoles
            .Select(r =>
            {
                var code = (r.CodigoRol ?? string.Empty).Trim().ToUpperInvariant();
                return code switch
                {
                    "ADMINISTRADOR" or "ADMIN_SIST" or "TITULACION_ADMIN" => "TITULACION_ADMIN",
                    "DOCENTE" or "PROFESOR" or "TITULACION_DOCENTE" => "TITULACION_DOCENTE",
                    "ESTUDIANTE" or "ALUMNO" or "TITULACION_ESTUDIANTE" => "TITULACION_ESTUDIANTE",
                    _ => code
                };
            })
            .Where(r => !string.IsNullOrWhiteSpace(r))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (user.Administrador && !systemRoles.Contains("TITULACION_ADMIN", StringComparer.OrdinalIgnoreCase))
        {
            systemRoles.Add("TITULACION_ADMIN");
        }

        // Si es administrador y aún no tenía módulos asignados por roles, otorgar todos los módulos del sistema Titulación
        if (user.Administrador && modulesGrouped.Count == 0)
        {
            var systemModules = await _context.RbacModulos
                .AsNoTracking()
                .Include(m => m.RbacModulosOperaciones)
                    .ThenInclude(mo => mo.IdOperacionesNavigation)
                .Where(m => m.IdSistemaNavigation.Codigo == systemCode || m.IdSistemaNavigation.Codigo == "TITULACION")
                .ToListAsync(cancellationToken);

            modulesGrouped = systemModules.Select(m => new RbacModuloPermissionsDto
            {
                IdModulo = m.IdModulos,
                NombreModulo = m.Nombre ?? string.Empty,
                Operaciones = m.RbacModulosOperaciones
                    .Select(mo => mo.IdOperacionesNavigation?.NombreOperacion ?? string.Empty)
                    .Where(o => !string.IsNullOrWhiteSpace(o))
                    .Distinct()
                    .ToList()
            }).ToList();
        }

        return new UserPermissionsDto
        {
            IdUsuario = user.IdUsuario,
            Nombre = user.Nombre ?? string.Empty,
            EmailInstitucional = user.EmailInstitucional ?? string.Empty,
            IdSigafi = user.IdSigafi ?? string.Empty,
            TablaSigafi = user.TablaSigafi ?? string.Empty,
            Roles = systemRoles,
            Modulos = modulesGrouped
        };
    }

    public async Task<bool> HasPermissionAsync(int idUsuario, string moduleName, string operationName, CancellationToken cancellationToken = default)
    {
        var permissions = await BuildUserPermissionsAsync(idUsuario, "TITULACION", cancellationToken);
        var modulo = permissions.Modulos.FirstOrDefault(m => string.Equals(m.NombreModulo, moduleName, StringComparison.OrdinalIgnoreCase));
        if (modulo == null)
        {
            return false;
        }

        return modulo.Operaciones.Any(o => string.Equals(o, operationName, StringComparison.OrdinalIgnoreCase));
    }
}
