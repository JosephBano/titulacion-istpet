using Microsoft.EntityFrameworkCore;
using TitulacionIstpet.Application.DTOs.Auth;
using TitulacionIstpet.Application.Interfaces;
using TitulacionIstpet.Domain.Entities;
using TitulacionIstpet.Infrastructure.Persistence;

namespace TitulacionIstpet.Infrastructure.Services;

public class RbacManagementService : IRbacManagementService
{
    private readonly SigafiDbContext _context;

    public RbacManagementService(SigafiDbContext context)
    {
        _context = context;
    }

    public async Task<List<RbacSistemaDto>> GetSistemasAsync(CancellationToken cancellationToken = default)
    {
        return await _context.RbacSistemas
            .AsNoTracking()
            .Select(s => new RbacSistemaDto(
                s.IdSistema,
                s.Codigo,
                s.Detalle,
                s.Icono
            ))
            .ToListAsync(cancellationToken);
    }

    public async Task<List<RbacModuloDto>> GetModulosBySistemaAsync(int idSistema, CancellationToken cancellationToken = default)
    {
        var modulos = await _context.RbacModulos
            .AsNoTracking()
            .Where(m => m.IdSistema == idSistema && m.EsActivo == true)
            .Include(m => m.RbacModulosOperaciones)
                .ThenInclude(mo => mo.IdOperacionesNavigation)
            .ToListAsync(cancellationToken);

        return modulos.Select(m => new RbacModuloDto(
            m.IdModulos,
            m.Nombre,
            m.EsActivo,
            m.RbacModulosOperaciones.Select(mo => new RbacOperacionDto(
                mo.IdModulosOperaciones,
                mo.IdOperaciones,
                mo.IdOperacionesNavigation != null ? mo.IdOperacionesNavigation.NombreOperacion : string.Empty
            )).ToList()
        )).ToList();
    }

    public async Task<List<RbacRolDto>> GetRolesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.RbacRols
            .AsNoTracking()
            .Where(r => r.EsActivo == true)
            .Select(r => new RbacRolDto(
                r.IdRol,
                r.Nombre,
                r.CodigoRol,
                r.EsActivo
            ))
            .ToListAsync(cancellationToken);
    }

    public async Task<RbacRolDto> CreateRolAsync(string nombre, string codigoRol, CancellationToken cancellationToken = default)
    {
        var rol = new RbacRol
        {
            Nombre = nombre,
            CodigoRol = codigoRol.ToUpper().Trim(),
            EsActivo = true
        };

        _context.RbacRols.Add(rol);
        await _context.SaveChangesAsync(cancellationToken);

        return new RbacRolDto(rol.IdRol, rol.Nombre, rol.CodigoRol, rol.EsActivo);
    }

    public async Task<bool> AssignRolToUsuarioAsync(int idUsuario, int idRol, CancellationToken cancellationToken = default)
    {
        var existingAssignment = await _context.RbacUsuarioRols
            .FirstOrDefaultAsync(ur => ur.IdUsuario == idUsuario && ur.IdRol == idRol, cancellationToken);

        if (existingAssignment != null)
        {
            existingAssignment.EsActivo = true;
            existingAssignment.FechaModificacion = DateOnly.FromDateTime(DateTime.UtcNow);
        }
        else
        {
            _context.RbacUsuarioRols.Add(new RbacUsuarioRol
            {
                IdUsuario = idUsuario,
                IdRol = idRol,
                EsActivo = true,
                FechaCreacion = DateOnly.FromDateTime(DateTime.UtcNow)
            });
        }

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> RemoveRolFromUsuarioAsync(int idUsuario, int idRol, CancellationToken cancellationToken = default)
    {
        var assignment = await _context.RbacUsuarioRols
            .FirstOrDefaultAsync(ur => ur.IdUsuario == idUsuario && ur.IdRol == idRol, cancellationToken);

        if (assignment == null)
        {
            return false;
        }

        assignment.EsActivo = false;
        assignment.FechaModificacion = DateOnly.FromDateTime(DateTime.UtcNow);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> AssignPermissionToRolAsync(int idRol, int idModuloOperacion, CancellationToken cancellationToken = default)
    {
        var existing = await _context.RbacRolModuloOperacions
            .FirstOrDefaultAsync(rmo => rmo.IdRol == idRol && rmo.IdModulosOperaciones == idModuloOperacion, cancellationToken);

        if (existing != null)
        {
            existing.EsActivo = true;
            existing.FechaModificacion = DateOnly.FromDateTime(DateTime.UtcNow);
        }
        else
        {
            _context.RbacRolModuloOperacions.Add(new RbacRolModuloOperacion
            {
                IdRol = idRol,
                IdModulosOperaciones = idModuloOperacion,
                EsActivo = true,
                FechaAsignacion = DateOnly.FromDateTime(DateTime.UtcNow)
            });
        }

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
