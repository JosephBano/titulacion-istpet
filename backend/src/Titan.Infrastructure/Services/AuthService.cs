using Microsoft.EntityFrameworkCore;
using Titan.Application.DTOs.Auth;
using Titan.Application.Interfaces;
using Titan.Domain.Entities;
using Titan.Domain.Interfaces.Security;
using Titan.Infrastructure.Persistence;

namespace Titan.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly TitanDbContext _context;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly IRbacService _rbacService;

    public AuthService(
        TitanDbContext context,
        IPasswordHasher passwordHasher,
        IJwtTokenGenerator jwtTokenGenerator,
        IRbacService rbacService)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _jwtTokenGenerator = jwtTokenGenerator;
        _rbacService = rbacService;
    }

    public async Task<LoginResponseDto> LoginAsync(LoginRequestDto request, string ipAddress, string deviceInfo, CancellationToken cancellationToken = default)
    {
        var term = (request.UsernameOrEmail ?? string.Empty).Trim();

        Console.WriteLine($"\n[AUTH-DEBUG] INTENTO DE LOGIN -> term: '{term}', SystemCode: '{request.SystemCode}'");

        var user = await _context.usuarios
            .FirstOrDefaultAsync(u =>
                u.idSigafi == term ||
                u.emailInstitucional == term ||
                u.nombre == term ||
                EF.Functions.Like(u.idSigafi, term) ||
                EF.Functions.Like(u.emailInstitucional, term),
                cancellationToken);

        if (user == null)
        {
            Console.WriteLine($"[AUTH-DEBUG] Usuario '{term}' NO encontrado en 'usuarios'. Buscando auto-aprovisionamiento JIT en 'profesores' y 'alumnos'...");

            // 1. Intentar autenticar contra la tabla 'profesores'
            var profesor = await _context.profesores
                .FirstOrDefaultAsync(p =>
                    p.idProfesor == term ||
                    p.emailInstitucional == term ||
                    p.email == term,
                    cancellationToken);

            if (profesor != null && _passwordHasher.VerifyPassword(request.Password, profesor.clave))
            {
                Console.WriteLine($"[AUTH-DEBUG] JIT: Profesor '{profesor.idProfesor}' verificado con éxito. Migrando a 'usuarios'...");
                string nombreProfesor = $"{profesor.primerNombre} {profesor.primerApellido}".Trim();
                if (string.IsNullOrWhiteSpace(nombreProfesor))
                {
                    nombreProfesor = $"{profesor.nombres} {profesor.apellidos}".Trim();
                }

                user = new usuarios
                {
                    idSigafi = profesor.idProfesor,
                    tablaSigafi = "profesor",
                    nombre = string.IsNullOrWhiteSpace(nombreProfesor) ? profesor.idProfesor : nombreProfesor,
                    emailInstitucional = !string.IsNullOrWhiteSpace(profesor.emailInstitucional) ? profesor.emailInstitucional : profesor.email,
                    contrasenia = _passwordHasher.HashPassword(request.Password),
                    activo = 1,
                    administrador = 0
                };

                _context.usuarios.Add(user);
                await _context.SaveChangesAsync(cancellationToken);

                var rolDocente = await _context.rbac_rol
                    .FirstOrDefaultAsync(r => r.codigo_rol == "TITAN_DOCENTE", cancellationToken);
                if (rolDocente != null)
                {
                    _context.rbac_usuario_rol.Add(new rbac_usuario_rol
                    {
                        idUsuario = user.idUsuario,
                        idRol = rolDocente.idRol,
                        esActivo = 1,
                        fecha_creacion = DateOnly.FromDateTime(DateTime.Now)
                    });
                    await _context.SaveChangesAsync(cancellationToken);
                }
            }

            // 2. Si no es profesor, intentar autenticar contra la tabla 'alumnos'
            if (user == null)
            {
                var alumno = await _context.alumnos
                    .FirstOrDefaultAsync(a =>
                        a.idAlumno == term ||
                        a.user_alumno == term ||
                        a.email_institucional == term ||
                        a.email == term,
                        cancellationToken);

                if (alumno != null && _passwordHasher.VerifyPassword(request.Password, alumno.password))
                {
                    Console.WriteLine($"[AUTH-DEBUG] JIT: Alumno '{alumno.idAlumno}' verificado con éxito. Migrando a 'usuarios'...");
                    string nombreAlumno = $"{alumno.primerNombre} {alumno.apellidoPaterno}".Trim();

                    user = new usuarios
                    {
                        idSigafi = alumno.idAlumno,
                        tablaSigafi = "alumno",
                        nombre = string.IsNullOrWhiteSpace(nombreAlumno) ? alumno.idAlumno : nombreAlumno,
                        emailInstitucional = !string.IsNullOrWhiteSpace(alumno.email_institucional) ? alumno.email_institucional : alumno.email,
                        contrasenia = _passwordHasher.HashPassword(request.Password),
                        activo = 1,
                        administrador = 0
                    };

                    _context.usuarios.Add(user);
                    await _context.SaveChangesAsync(cancellationToken);

                    var rolEstudiante = await _context.rbac_rol
                        .FirstOrDefaultAsync(r => r.codigo_rol == "TITAN_ESTUDIANTE", cancellationToken);
                    if (rolEstudiante != null)
                    {
                        _context.rbac_usuario_rol.Add(new rbac_usuario_rol
                        {
                            idUsuario = user.idUsuario,
                            idRol = rolEstudiante.idRol,
                            esActivo = 1,
                            fecha_creacion = DateOnly.FromDateTime(DateTime.Now)
                        });
                        await _context.SaveChangesAsync(cancellationToken);
                    }
                }
            }

            if (user == null)
            {
                Console.WriteLine($"[AUTH-DEBUG] ERROR: Usuario '{request.UsernameOrEmail}' NO existe o la contraseña es incorrecta.");
                throw new UnauthorizedAccessException("Credenciales de acceso inválidas.");
            }
        }

        Console.WriteLine($"[AUTH-DEBUG] Usuario encontrado -> idUsuario: {user.idUsuario}, idSigafi: '{user.idSigafi}', activo: {user.activo}");

        if (user.activo != 1)
        {
            Console.WriteLine($"[AUTH-DEBUG] ERROR: El usuario {user.idUsuario} está inactivo (activo = {user.activo}).");
            throw new UnauthorizedAccessException("El usuario se encuentra inactivo en el sistema.");
        }

        bool passValid = _passwordHasher.VerifyPassword(request.Password, user.contrasenia);
        Console.WriteLine($"[AUTH-DEBUG] Verificación de Contraseña en 'usuarios' -> Input: '{request.Password}', EnDB: '{user.contrasenia}', Válida: {passValid}");

        // Sincronización JIT de contraseñas actualizadas en tablas legadas de SIGAFI (profesores / alumnos)
        if (!passValid && !string.IsNullOrWhiteSpace(user.idSigafi))
        {
            var profesorLegacy = await _context.profesores
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.idProfesor == user.idSigafi, cancellationToken);

            if (profesorLegacy != null && (profesorLegacy.clave == request.Password || _passwordHasher.VerifyPassword(request.Password, profesorLegacy.clave)))
            {
                Console.WriteLine($"[AUTH-DEBUG] Sincronización de Contraseña SIGAFI -> Profesor '{user.idSigafi}' actualizó clave legada. Sincronizando hash...");
                user.contrasenia = _passwordHasher.HashPassword(request.Password);
                await _context.SaveChangesAsync(cancellationToken);
                passValid = true;
            }
            else
            {
                var alumnoLegacy = await _context.alumnos
                    .AsNoTracking()
                    .FirstOrDefaultAsync(a => a.idAlumno == user.idSigafi, cancellationToken);

                if (alumnoLegacy != null && (alumnoLegacy.password == request.Password || _passwordHasher.VerifyPassword(request.Password, alumnoLegacy.password)))
                {
                    Console.WriteLine($"[AUTH-DEBUG] Sincronización de Contraseña SIGAFI -> Alumno '{user.idSigafi}' actualizó clave legada. Sincronizando hash...");
                    user.contrasenia = _passwordHasher.HashPassword(request.Password);
                    await _context.SaveChangesAsync(cancellationToken);
                    passValid = true;
                }
            }
        }

        if (!passValid)
        {
            Console.WriteLine($"[AUTH-DEBUG] ERROR: La contraseña proporcionada no coincide.");
            throw new UnauthorizedAccessException("Credenciales de acceso inválidas.");
        }

        Console.WriteLine($"[AUTH-DEBUG] Construyendo permisos RBAC para idUsuario: {user.idUsuario}...");
        UserPermissionsDto permissions;
        try
        {
            permissions = await _rbacService.BuildUserPermissionsAsync(user.idUsuario, request.SystemCode, cancellationToken);
            Console.WriteLine($"[AUTH-DEBUG] Permisos RBAC construidos exitosamente. Modulos: {permissions.Modulos.Count}, Roles: {string.Join(", ", permissions.Roles)}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[AUTH-DEBUG] ERROR EN RBAC: {ex.Message}");
            throw;
        }

        // Evaluación 100% Dinámica en Memoria C# (Sin modificar la base de datos MySQL)
        var hoy = DateOnly.FromDateTime(DateTime.Now);

        // 1. Verificar dinámicamente si el usuario es un docente laboralmente ACTIVO
        bool esDocenteActivo = await _context.profesores.AsNoTracking().AnyAsync(p =>
            p.idProfesor == user.idSigafi &&
            (p.activo == 1 || p.activo == null) &&
            (p.fecha_retiro == null || p.fecha_retiro > hoy), cancellationToken);

        // Si el profesor está INACTIVO o RETIRADO, revocar dinámicamente el rol docente en memoria
        if (!esDocenteActivo)
        {
            permissions.Roles.RemoveAll(r =>
                r.Equals("TITAN_DOCENTE", StringComparison.OrdinalIgnoreCase) ||
                r.Equals("DOCENTE", StringComparison.OrdinalIgnoreCase) ||
                r.Equals("PROFESOR", StringComparison.OrdinalIgnoreCase));
        }

        bool esDocente = esDocenteActivo;

        // 2. Determinar si el usuario es Administrador del Sistema Titán
        bool esAdministradorTitan = permissions.Roles.Any(r =>
            r.Equals("TITAN_ADMIN", StringComparison.OrdinalIgnoreCase) ||
            r.Equals("TITAN_ADMINISTRADOR", StringComparison.OrdinalIgnoreCase) ||
            r.Equals("ADMINISTRADOR", StringComparison.OrdinalIgnoreCase) ||
            r.Equals("ADMIN_SIST", StringComparison.OrdinalIgnoreCase));

        // 3. Determinar si la persona está registrada como Estudiante
        bool esEstudiante = string.Equals(user.tablaSigafi, "alumno", StringComparison.OrdinalIgnoreCase) ||
                            string.Equals(user.tablaSigafi, "alumnos", StringComparison.OrdinalIgnoreCase) ||
                            await _context.alumnos.AsNoTracking().AnyAsync(a => a.idAlumno == user.idSigafi, cancellationToken);

        // Si el usuario NO es Administrador, NO es Docente Activo y NO es Estudiante (ej. Docente Retirado sin perfil de alumno):
        if (!esAdministradorTitan && !esDocente && !esEstudiante)
        {
            throw new UnauthorizedAccessException("Su cuenta se encuentra inactiva por retiro o desvinculación institucional.");
        }

        bool esEstudiantePuro = esEstudiante && !esDocente && !esAdministradorTitan;

        // Para todo estudiante puro (no docente, no admin), aplicar las reglas de bloqueo por titulación
        if (esEstudiantePuro)
        {
            var carrerasTituladasIds = await _context.alumnos_titulos
                .AsNoTracking()
                .Where(at => at.idAlumno == user.idSigafi)
                .Join(_context.titulos, at => at.idTitulo, t => t.idTitulo, (at, t) => t.idCarrera)
                .Where(idCarrera => idCarrera.HasValue)
                .Select(idCarrera => idCarrera.Value)
                .Distinct()
                .ToListAsync(cancellationToken);

            bool tieneMatriculaActiva = await _context.matriculas
                .AsNoTracking()
                .AnyAsync(m => m.idAlumno == user.idSigafi && (m.retirado == false || m.retirado == null), cancellationToken);

            if (!tieneMatriculaActiva)
            {
                Console.WriteLine($"[AUTH-DEBUG] LOGIN BLOQUEADO: El estudiante '{user.idSigafi}' no registra ninguna matrícula activa.");
                throw new UnauthorizedAccessException("Estimado estudiante, no registra una matrícula activa en la institución para el periodo académico vigente. Contacte a Secretaría Académica.");
            }

            // Si el estudiante ya cuenta con un título registrado en la institución
            if (carrerasTituladasIds.Count > 0)
            {
                var carrerasAlumno = await _context.alumnos_carreras
                    .AsNoTracking()
                    .Where(ac => ac.idAlumno == user.idSigafi)
                    .Select(ac => ac.idCarrera)
                    .ToListAsync(cancellationToken);

                bool tieneNuevaCarreraPendiente = carrerasAlumno.Any(idC => !carrerasTituladasIds.Contains(idC));

                if (!tieneNuevaCarreraPendiente)
                {
                    Console.WriteLine($"[AUTH-DEBUG] LOGIN BLOQUEADO: El estudiante '{user.idSigafi}' ya se encuentra titulado en su(s) carrera(s) registrada(s).");
                    throw new UnauthorizedAccessException("Estimado estudiante, usted ya se encuentra titulado en su(s) carrera(s) registrada(s) y no registra matrículas activas en una nueva carrera.");
                }
            }
        }
        var allPermissionsList = permissions.Modulos
            .SelectMany(m => m.Operaciones.Select(o => $"{m.NombreModulo}:{o}"))
            .ToList();

        var (accessToken, expiresAt) = _jwtTokenGenerator.GenerateAccessToken(user, permissions.Roles, allPermissionsList);
        var refreshTokenValue = _jwtTokenGenerator.GenerateRefreshToken();
        var refreshTokenHash = _jwtTokenGenerator.HashToken(refreshTokenValue);

        var refreshTokenEntity = new rbac_refresh_tokens
        {
            idUsuario = user.idUsuario,
            tokenHash = refreshTokenHash,
            deviceInfo = deviceInfo,
            ipAddress = ipAddress,
            createdAt = DateTime.UtcNow,
            expiresAt = DateTime.UtcNow.AddDays(7),
            familyId = Guid.NewGuid().ToString()
        };

        _context.rbac_refresh_tokens.Add(refreshTokenEntity);
        await _context.SaveChangesAsync(cancellationToken);

        return new LoginResponseDto
        {
            AccessToken = accessToken,
            RefreshToken = refreshTokenValue,
            ExpiresAt = expiresAt,
            UserInfo = permissions
        };
    }

    public async Task<LoginResponseDto> RefreshTokenAsync(RefreshTokenRequestDto request, string ipAddress, string deviceInfo, CancellationToken cancellationToken = default)
    {
        var tokenHash = _jwtTokenGenerator.HashToken(request.RefreshToken);

        var tokenEntity = await _context.rbac_refresh_tokens
            .Include(t => t.idUsuarioNavigation)
            .FirstOrDefaultAsync(t => t.tokenHash == tokenHash, cancellationToken);

        if (tokenEntity == null || tokenEntity.revokedAt != null || tokenEntity.expiresAt <= DateTime.UtcNow)
        {
            throw new UnauthorizedAccessException("El refresh token es inválido o ha expirado.");
        }

        var user = tokenEntity.idUsuarioNavigation;
        if (user.activo != 1)
        {
            throw new UnauthorizedAccessException("El usuario se encuentra inactivo.");
        }

        // Revocar token anterior (Rotation)
        tokenEntity.revokedAt = DateTime.UtcNow;
        tokenEntity.revokedReason = "Reemplazado por nuevo token (rotation)";

        var permissions = await _rbacService.BuildUserPermissionsAsync(user.idUsuario, "TITAN", cancellationToken);
        var allPermissionsList = permissions.Modulos
            .SelectMany(m => m.Operaciones.Select(o => $"{m.NombreModulo}:{o}"))
            .ToList();

        var (newAccessToken, expiresAt) = _jwtTokenGenerator.GenerateAccessToken(user, permissions.Roles, allPermissionsList);
        var newRefreshTokenValue = _jwtTokenGenerator.GenerateRefreshToken();
        var newRefreshTokenHash = _jwtTokenGenerator.HashToken(newRefreshTokenValue);

        var newRefreshTokenEntity = new rbac_refresh_tokens
        {
            idUsuario = user.idUsuario,
            tokenHash = newRefreshTokenHash,
            deviceInfo = deviceInfo,
            ipAddress = ipAddress,
            createdAt = DateTime.UtcNow,
            expiresAt = DateTime.UtcNow.AddDays(7),
            familyId = tokenEntity.familyId
        };

        _context.rbac_refresh_tokens.Add(newRefreshTokenEntity);
        await _context.SaveChangesAsync(cancellationToken);

        return new LoginResponseDto
        {
            AccessToken = newAccessToken,
            RefreshToken = newRefreshTokenValue,
            ExpiresAt = expiresAt,
            UserInfo = permissions
        };
    }

    public async Task<bool> RevokeRefreshTokenAsync(string refreshToken, string reason, CancellationToken cancellationToken = default)
    {
        var tokenHash = _jwtTokenGenerator.HashToken(refreshToken);
        var tokenEntity = await _context.rbac_refresh_tokens
            .FirstOrDefaultAsync(t => t.tokenHash == tokenHash, cancellationToken);

        if (tokenEntity == null || tokenEntity.revokedAt != null)
        {
            return false;
        }

        var safeReason = string.IsNullOrWhiteSpace(reason)
            ? "Cierre de sesión"
            : (reason.Length > 30 ? reason[..30] : reason);

        tokenEntity.revokedAt = DateTime.UtcNow;
        tokenEntity.revokedReason = safeReason;

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<UserPermissionsDto> GetUserPermissionsAsync(int idUsuario, string systemCode = "TITAN", CancellationToken cancellationToken = default)
    {
        return await _rbacService.BuildUserPermissionsAsync(idUsuario, systemCode, cancellationToken);
    }
}
