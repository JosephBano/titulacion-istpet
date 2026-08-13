using Microsoft.EntityFrameworkCore;
using Titan.Application.Interfaces;
using Titan.Domain.Entities;
using Titan.Infrastructure.Persistence;

namespace Titan.Infrastructure.Services;

public class RbacManagementService : IRbacManagementService
{
    private readonly TitanDbContext _context;

    public RbacManagementService(TitanDbContext context)
    {
        _context = context;
    }

    public async Task<List<rbac_sistema>> GetSistemasAsync(CancellationToken cancellationToken = default)
    {
        return await _context.rbac_sistema
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<List<rbac_modulos>> GetModulosBySistemaAsync(int idSistema, CancellationToken cancellationToken = default)
    {
        return await _context.rbac_modulos
            .AsNoTracking()
            .Where(m => m.id_sistema == idSistema && m.esActivo == 1)
            .Include(m => m.rbac_modulos_operaciones)
                .ThenInclude(mo => mo.idOperacionesNavigation)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<rbac_rol>> GetRolesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.rbac_rol
            .AsNoTracking()
            .Where(r => r.esActivo == 1)
            .ToListAsync(cancellationToken);
    }

    public async Task<rbac_rol> CreateRolAsync(string nombre, string codigoRol, CancellationToken cancellationToken = default)
    {
        var rol = new rbac_rol
        {
            Nombre = nombre,
            codigo_rol = codigoRol.ToUpper().Trim(),
            esActivo = 1
        };

        _context.rbac_rol.Add(rol);
        await _context.SaveChangesAsync(cancellationToken);
        return rol;
    }

    public async Task<bool> AssignRolToUsuarioAsync(int idUsuario, int idRol, CancellationToken cancellationToken = default)
    {
        var existingAssignment = await _context.rbac_usuario_rol
            .FirstOrDefaultAsync(ur => ur.idUsuario == idUsuario && ur.idRol == idRol, cancellationToken);

        if (existingAssignment != null)
        {
            existingAssignment.esActivo = 1;
            existingAssignment.fecha_modificacion = DateOnly.FromDateTime(DateTime.UtcNow);
        }
        else
        {
            _context.rbac_usuario_rol.Add(new rbac_usuario_rol
            {
                idUsuario = idUsuario,
                idRol = idRol,
                esActivo = 1,
                fecha_creacion = DateOnly.FromDateTime(DateTime.UtcNow)
            });
        }

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> RemoveRolFromUsuarioAsync(int idUsuario, int idRol, CancellationToken cancellationToken = default)
    {
        var assignment = await _context.rbac_usuario_rol
            .FirstOrDefaultAsync(ur => ur.idUsuario == idUsuario && ur.idRol == idRol, cancellationToken);

        if (assignment == null)
        {
            return false;
        }

        assignment.esActivo = 0;
        assignment.fecha_modificacion = DateOnly.FromDateTime(DateTime.UtcNow);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> AssignPermissionToRolAsync(int idRol, int idModuloOperacion, CancellationToken cancellationToken = default)
    {
        var existing = await _context.rbac_rol_modulo_operacion
            .FirstOrDefaultAsync(rmo => rmo.idRol == idRol && rmo.idModulosOperaciones == idModuloOperacion, cancellationToken);

        if (existing != null)
        {
            existing.esActivo = 1;
            existing.fecha_modificacion = DateOnly.FromDateTime(DateTime.UtcNow);
        }
        else
        {
            _context.rbac_rol_modulo_operacion.Add(new rbac_rol_modulo_operacion
            {
                idRol = idRol,
                idModulosOperaciones = idModuloOperacion,
                esActivo = 1,
                fecha_asignacion = DateOnly.FromDateTime(DateTime.UtcNow)
            });
        }

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
