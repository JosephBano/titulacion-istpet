using Microsoft.EntityFrameworkCore;
using Titan.Domain.Entities;
using Titan.Domain.Interfaces.Security;
using Titan.Infrastructure.Persistence;

namespace Titan.Infrastructure.Persistence;

public static class RbacDbSeeder
{
    public static async Task SeedAsync(TitanDbContext context, IPasswordHasher passwordHasher)
    {
        // 1. Sistema principal TITAN
        var sistema = await context.rbac_sistema.FirstOrDefaultAsync(s => s.codigo == "TITAN");
        if (sistema == null)
        {
            sistema = new rbac_sistema
            {
                codigo = "TITAN",
                detalle = "Sistema de Titulación Académica ISTPET",
                url = "http://localhost:4200",
                icono = "academic-cap"
            };
            context.rbac_sistema.Add(sistema);
            await context.SaveChangesAsync();
        }

        // 2. Módulos funcionales del sistema TITAN
        var modulosNombres = new Dictionary<string, string>
        {
            { "TITAN_CONFIG", "Configuración y Seguridad RBAC" },
            { "TITAN_ACTORES", "Gestión de Alumnos y Docentes" },
            { "TITAN_POSTULACIONES", "Postulaciones de Titulación" },
            { "TITAN_COMPLEXIVO", "Examen Complexivo" },
            { "TITAN_TRABAJO_GRADO", "Trabajos de Integración Curricular" },
            { "TITAN_DEFENSAS", "Tribunales y Defensas de Grado" },
            { "TITAN_ACTAS", "Actas de Grado y Titulación" }
        };

        var modulosEntities = new Dictionary<string, rbac_modulos>();
        foreach (var (codigoMod, nombreMod) in modulosNombres)
        {
            var modulo = await context.rbac_modulos.FirstOrDefaultAsync(m => m.Nombre == nombreMod && m.id_sistema == sistema.idSistema);
            if (modulo == null)
            {
                modulo = new rbac_modulos
                {
                    id_sistema = sistema.idSistema,
                    Nombre = nombreMod,
                    esActivo = 1
                };
                context.rbac_modulos.Add(modulo);
                await context.SaveChangesAsync();
            }
            modulosEntities[codigoMod] = modulo;
        }

        // 3. Operaciones atómicas maestras
        var operacionesNombres = new[]
        {
            "CONSULTAR", "CREAR", "EDITAR", "ELIMINAR",
            "APROBAR", "RECHAZAR", "ASIGNAR_TUTOR", "REGISTRAR_NOTAS", "SUSCRIBIR_ACTA"
        };

        var operacionesEntities = new Dictionary<string, rbac_operaciones>();
        foreach (var opNombre in operacionesNombres)
        {
            var op = await context.rbac_operaciones.FirstOrDefaultAsync(o => o.NombreOperacion == opNombre);
            if (op == null)
            {
                op = new rbac_operaciones
                {
                    NombreOperacion = opNombre
                };
                context.rbac_operaciones.Add(op);
                await context.SaveChangesAsync();
            }
            operacionesEntities[opNombre] = op;
        }

        // 4. Relación Módulos - Operaciones
        var modulosOpsEntities = new List<rbac_modulos_operaciones>();
        foreach (var (modKey, modulo) in modulosEntities)
        {
            foreach (var (opKey, op) in operacionesEntities)
            {
                var mo = await context.rbac_modulos_operaciones
                    .FirstOrDefaultAsync(m => m.idModulos == modulo.idModulos && m.idOperaciones == op.idOperaciones);

                if (mo == null)
                {
                    mo = new rbac_modulos_operaciones
                    {
                        idModulos = modulo.idModulos,
                        idOperaciones = op.idOperaciones,
                        esActivo = 1,
                        fecha_creacion = DateOnly.FromDateTime(DateTime.UtcNow)
                    };
                    context.rbac_modulos_operaciones.Add(mo);
                    await context.SaveChangesAsync();
                }
                modulosOpsEntities.Add(mo);
            }
        }

        // 5. Roles Institucionales Estandarizados (TITAN_)
        var rolesDefiniciones = new Dictionary<string, string>
        {
            { "TITAN_ADMINISTRADOR", "Administrador General del Sistema" },
            { "TITAN_DOCENTE", "Docente Tutor y Evaluador de Tribunal" },
            { "TITAN_ESTUDIANTE", "Estudiante Postulante de Titulación" }
        };

        var rolesEntities = new Dictionary<string, rbac_rol>();
        foreach (var (codigoRol, nombreRol) in rolesDefiniciones)
        {
            var rol = await context.rbac_rol.FirstOrDefaultAsync(r => r.codigo_rol == codigoRol);
            if (rol == null)
            {
                rol = new rbac_rol
                {
                    codigo_rol = codigoRol,
                    Nombre = nombreRol,
                    esActivo = 1
                };
                context.rbac_rol.Add(rol);
                await context.SaveChangesAsync();
            }
            rolesEntities[codigoRol] = rol;
        }

        // 6. Matriz de Permisos (rbac_rol_modulo_operacion)
        // TITAN_ADMINISTRADOR -> Todos los permisos
        var adminRol = rolesEntities["TITAN_ADMINISTRADOR"];
        foreach (var mo in modulosOpsEntities)
        {
            await AssignPermissionIfNotExistAsync(context, adminRol.idRol, mo.idModulosOperaciones);
        }

        // TITAN_DOCENTE -> Consultar, Editar avances, Registrar Notas, Suscribir Actas
        var docenteRol = rolesEntities["TITAN_DOCENTE"];
        var docentePermisosOps = new[] { "CONSULTAR", "EDITAR", "REGISTRAR_NOTAS", "SUSCRIBIR_ACTA" };
        foreach (var mo in modulosOpsEntities)
        {
            var opName = operacionesNombres.FirstOrDefault(opKey => operacionesEntities[opKey].idOperaciones == mo.idOperaciones);
            if (opName != null && docentePermisosOps.Contains(opName))
            {
                await AssignPermissionIfNotExistAsync(context, docenteRol.idRol, mo.idModulosOperaciones);
            }
        }

        // TITAN_ESTUDIANTE -> Consultar, Crear postulación
        var estudianteRol = rolesEntities["TITAN_ESTUDIANTE"];
        var estudiantePermisosOps = new[] { "CONSULTAR", "CREAR" };
        foreach (var mo in modulosOpsEntities)
        {
            var opName = operacionesNombres.FirstOrDefault(opKey => operacionesEntities[opKey].idOperaciones == mo.idOperaciones);
            if (opName != null && estudiantePermisosOps.Contains(opName))
            {
                await AssignPermissionIfNotExistAsync(context, estudianteRol.idRol, mo.idModulosOperaciones);
            }
        }

        // 7. Usuario Administrador Inicial
        var adminEmail = "admin@istpet.edu.ec";
        var adminUser = await context.usuarios.FirstOrDefaultAsync(u => u.emailInstitucional == adminEmail || u.nombre == "admin");

        if (adminUser == null)
        {
            adminUser = new usuarios
            {
                nombre = "Administrador General",
                emailInstitucional = adminEmail,
                contrasenia = passwordHasher.HashPassword("Admin123!*"),
                activo = 1,
                administrador = 1,
                idSigafi = "ADMIN001",
                tablaSigafi = "administrador",
                emailValidado = 1
            };
            context.usuarios.Add(adminUser);
            await context.SaveChangesAsync();
        }

        // Asignar rol TITAN_ADMINISTRADOR al usuario Admin
        var userRolExist = await context.rbac_usuario_rol
            .FirstOrDefaultAsync(ur => ur.idUsuario == adminUser.idUsuario && ur.idRol == adminRol.idRol);

        if (userRolExist == null)
        {
            context.rbac_usuario_rol.Add(new rbac_usuario_rol
            {
                idUsuario = adminUser.idUsuario,
                idRol = adminRol.idRol,
                esActivo = 1,
                fecha_creacion = DateOnly.FromDateTime(DateTime.UtcNow)
            });
            await context.SaveChangesAsync();
        }
    }

    private static async Task AssignPermissionIfNotExistAsync(TitanDbContext context, int idRol, int idModuloOperacion)
    {
        var exist = await context.rbac_rol_modulo_operacion
            .FirstOrDefaultAsync(rmo => rmo.idRol == idRol && rmo.idModulosOperaciones == idModuloOperacion);

        if (exist == null)
        {
            context.rbac_rol_modulo_operacion.Add(new rbac_rol_modulo_operacion
            {
                idRol = idRol,
                idModulosOperaciones = idModuloOperacion,
                esActivo = 1,
                fecha_asignacion = DateOnly.FromDateTime(DateTime.UtcNow)
            });
            await context.SaveChangesAsync();
        }
    }
}
