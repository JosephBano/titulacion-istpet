using Microsoft.EntityFrameworkCore;
using TitulacionIstpet.Application.DTOs.Auth;
using TitulacionIstpet.Application.Interfaces;
using TitulacionIstpet.Domain.Entities;
using TitulacionIstpet.Domain.Interfaces.Security;
using TitulacionIstpet.Infrastructure.Persistence;

namespace TitulacionIstpet.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly SigafiDbContext _context;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly IRbacService _rbacService;

    public AuthService(
        SigafiDbContext context,
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
        var cleanInput = request.UsernameOrEmail.Trim();

        var user = await _context.Usuarios
            .FirstOrDefaultAsync(u =>
                (u.EmailInstitucional != null && u.EmailInstitucional == cleanInput) ||
                u.IdSigafi == cleanInput ||
                (u.Nombre != null && u.Nombre == cleanInput),
                cancellationToken);

        if (user == null)
        {
            // 1. Auto-aprovisionamiento si existe en la tabla Alumnos
            var alumno = await _context.Alumnos
                .FirstOrDefaultAsync(a =>
                    a.IdAlumno == cleanInput ||
                    (a.UserAlumno != null && a.UserAlumno == cleanInput) ||
                    (a.EmailInstitucional != null && a.EmailInstitucional == cleanInput) ||
                    (a.Email != null && a.Email == cleanInput),
                    cancellationToken);

            if (alumno != null && _passwordHasher.VerifyPassword(request.Password, alumno.Password))
            {
                // Validar que el estudiante pertenezca al último nivel o sea egresado apto para titulación
                var matriculasAlumno = await _context.Matriculas
                    .AsNoTracking()
                    .Include(m => m.IdNivelNavigation)
                    .Where(m => m.IdAlumno == alumno.IdAlumno && (m.Retirado == null || m.Retirado == false))
                    .ToListAsync(cancellationToken);

                if (matriculasAlumno.Count == 0)
                {
                    throw new UnauthorizedAccessException("Estimado estudiante, no registra matrículas activas en la institución.");
                }

                var carrerasAlumnoIds = matriculasAlumno
                    .Where(m => m.IdNivelNavigation != null && m.IdNivelNavigation.IdCarrera > 0)
                    .Select(m => m.IdNivelNavigation!.IdCarrera)
                    .Distinct()
                    .ToList();

                var nivelesMaximosPorCarrera = await _context.Cursos
                    .AsNoTracking()
                    .Where(c => carrerasAlumnoIds.Contains(c.IdCarrera))
                    .GroupBy(c => c.IdCarrera)
                    .Select(g => new { IdCarrera = g.Key, MaxOrden = g.Max(c => c.Orden ?? 0) })
                    .ToDictionaryAsync(x => x.IdCarrera, x => x.MaxOrden, cancellationToken);

                bool esAptoTitulacion = false;
                int nivelActualMax = 0;
                string nombreNivelActual = string.Empty;

                foreach (var mat in matriculasAlumno)
                {
                    if (mat.IdNivelNavigation != null)
                    {
                        int idCarrera = mat.IdNivelNavigation.IdCarrera;
                        int ordenMatricula = mat.IdNivelNavigation.Orden ?? 0;
                        int maxNivelCarrera = nivelesMaximosPorCarrera.GetValueOrDefault(idCarrera, 5);

                        if (ordenMatricula > nivelActualMax)
                        {
                            nivelActualMax = ordenMatricula;
                            nombreNivelActual = mat.IdNivelNavigation.Nivel ?? $"Nivel {ordenMatricula}";
                        }

                        if (ordenMatricula >= maxNivelCarrera || ordenMatricula >= 4)
                        {
                            esAptoTitulacion = true;
                            break;
                        }
                    }
                }

                if (!esAptoTitulacion)
                {
                    bool tienePostulacionPrevia = await _context.TitulPostulacionAlumnos
                        .AsNoTracking()
                        .AnyAsync(p => p.IdMatriculaNavigation.IdAlumno == alumno.IdAlumno, cancellationToken);

                    if (tienePostulacionPrevia)
                    {
                        esAptoTitulacion = true;
                    }
                }

                if (!esAptoTitulacion)
                {
                    var nivelMsg = !string.IsNullOrWhiteSpace(nombreNivelActual) ? $" (Nivel registrado: {nombreNivelActual})" : string.Empty;
                    throw new UnauthorizedAccessException($"Estimado estudiante, el Sistema de Titulación está habilitado exclusivamente para estudiantes del último semestre o egresados{nivelMsg}.");
                }

                var nombreCompleto = $"{alumno.PrimerNombre} {alumno.ApellidoPaterno}".Trim();
                if (string.IsNullOrWhiteSpace(nombreCompleto))
                {
                    nombreCompleto = alumno.UserAlumno ?? alumno.IdAlumno;
                }

                var emailInst = !string.IsNullOrWhiteSpace(alumno.EmailInstitucional)
                    ? alumno.EmailInstitucional
                    : (!string.IsNullOrWhiteSpace(alumno.Email) ? alumno.Email : $"{alumno.IdAlumno}@istpet.edu.ec");

                user = new Usuarios
                {
                    IdSigafi = alumno.IdAlumno,
                    TablaSigafi = "alumno",
                    Nombre = nombreCompleto,
                    Contrasenia = _passwordHasher.HashPassword(request.Password),
                    Activo = true,
                    Administrador = false,
                    EmailInstitucional = emailInst,
                    EmailValidado = true
                };

                _context.Usuarios.Add(user);
                await _context.SaveChangesAsync(cancellationToken);

                // Asignar rol institucional de alumno (Id 15)
                var rolEstudiante = await _context.RbacRol
                    .FirstOrDefaultAsync(r => r.CodigoRol == "alumno" || r.IdRol == 15 || r.CodigoRol == "ESTUDIANTE", cancellationToken);

                if (rolEstudiante != null)
                {
                    _context.RbacUsuarioRol.Add(new RbacUsuarioRol
                    {
                        IdUsuario = user.IdUsuario,
                        IdRol = rolEstudiante.IdRol,
                        EsActivo = true,
                        FechaCreacion = DateOnly.FromDateTime(DateTime.UtcNow)
                    });
                    await _context.SaveChangesAsync(cancellationToken);
                }
            }
            else
            {
                // 2. Auto-aprovisionamiento si existe en la tabla Profesores
                var profesor = await _context.Profesores
                    .FirstOrDefaultAsync(p =>
                        p.IdProfesor == cleanInput ||
                        (p.EmailInstitucional != null && p.EmailInstitucional == cleanInput) ||
                        (p.Email != null && p.Email == cleanInput),
                        cancellationToken);

                if (profesor != null && _passwordHasher.VerifyPassword(request.Password, profesor.Clave))
                {
                    if (profesor.Activo != true)
                    {
                        throw new UnauthorizedAccessException("El docente se encuentra inactivo en la institución.");
                    }

                    var nombreCompleto = $"{profesor.PrimerNombre} {profesor.Apellidos}".Trim();
                    if (string.IsNullOrWhiteSpace(nombreCompleto))
                    {
                        nombreCompleto = profesor.Nombres ?? profesor.IdProfesor;
                    }

                    var emailInst = !string.IsNullOrWhiteSpace(profesor.EmailInstitucional)
                        ? profesor.EmailInstitucional
                        : (!string.IsNullOrWhiteSpace(profesor.Email) ? profesor.Email : $"{profesor.IdProfesor}@istpet.edu.ec");

                    user = new Usuarios
                    {
                        IdSigafi = profesor.IdProfesor,
                        TablaSigafi = "profesor",
                        Nombre = nombreCompleto,
                        Contrasenia = _passwordHasher.HashPassword(request.Password),
                        Activo = true,
                        Administrador = false,
                        EmailInstitucional = emailInst,
                        EmailValidado = true
                    };

                    _context.Usuarios.Add(user);
                    await _context.SaveChangesAsync(cancellationToken);

                    // Asignar rol institucional de docente (Id 25)
                    var rolDocente = await _context.RbacRol
                        .FirstOrDefaultAsync(r => r.CodigoRol == "docente" || r.IdRol == 25 || r.CodigoRol == "PROFESOR" || r.CodigoRol == "DOCENTE", cancellationToken);

                    if (rolDocente != null)
                    {
                        _context.RbacUsuarioRol.Add(new RbacUsuarioRol
                        {
                            IdUsuario = user.IdUsuario,
                            IdRol = rolDocente.IdRol,
                            EsActivo = true,
                            FechaCreacion = DateOnly.FromDateTime(DateTime.UtcNow)
                        });
                        await _context.SaveChangesAsync(cancellationToken);
                    }
                }
                else
                {
                    throw new UnauthorizedAccessException("Credenciales de acceso inválidas.");
                }
            }
        }
        else
        {
            if (user.Activo != true)
            {
                throw new UnauthorizedAccessException("El usuario se encuentra inactivo.");
            }

            if (!_passwordHasher.VerifyPassword(request.Password, user.Contrasenia))
            {
                throw new UnauthorizedAccessException("Credenciales de acceso inválidas.");
            }

            // Migración progresiva de contraseña únicamente en la tabla Usuarios
            if (_passwordHasher.NeedsRehash(user.Contrasenia))
            {
                user.Contrasenia = _passwordHasher.HashPassword(request.Password);
                await _context.SaveChangesAsync(cancellationToken);
            }
        }

        var systemCode = string.IsNullOrWhiteSpace(request.SystemCode) ? "TITULACION" : request.SystemCode;
        var permissions = await _rbacService.BuildUserPermissionsAsync(user.IdUsuario, systemCode, cancellationToken);

        // 1. Determinar si el usuario es Docente activo en la institución
        bool esDocenteActivo = false;
        if (string.Equals(user.TablaSigafi, "profesor", StringComparison.OrdinalIgnoreCase))
        {
            var profesor = await _context.Profesores
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.IdProfesor == user.IdSigafi, cancellationToken);

            esDocenteActivo = profesor != null && (profesor.Activo == true);
        }
        else
        {
            esDocenteActivo = permissions.Roles.Any(r =>
                r.Equals("TITULACION_DOCENTE", StringComparison.OrdinalIgnoreCase) ||
                r.Equals("DOCENTE", StringComparison.OrdinalIgnoreCase) ||
                r.Equals("PROFESOR", StringComparison.OrdinalIgnoreCase));
        }

        bool esDocente = esDocenteActivo;

        // 2. Determinar si el usuario es Administrador del Sistema
        bool esAdministrador = user.Administrador == true || permissions.Roles.Any(r =>
            r.Equals("TITULACION_ADMIN", StringComparison.OrdinalIgnoreCase) ||
            r.Equals("ADMINISTRADOR", StringComparison.OrdinalIgnoreCase) ||
            r.Equals("ADMIN_SIST", StringComparison.OrdinalIgnoreCase));

        // 3. Determinar si la persona está registrada como Estudiante
        bool esEstudiante = string.Equals(user.TablaSigafi, "alumno", StringComparison.OrdinalIgnoreCase) ||
                            string.Equals(user.TablaSigafi, "alumnos", StringComparison.OrdinalIgnoreCase) ||
                            await _context.Alumnos.AsNoTracking().AnyAsync(a => a.IdAlumno == user.IdSigafi, cancellationToken);

        // Si el usuario NO es Administrador, NO es Docente Activo y NO es Estudiante:
        if (!esAdministrador && !esDocente && !esEstudiante)
        {
            throw new UnauthorizedAccessException("Su cuenta se encuentra inactiva por retiro o desvinculación institucional.");
        }

        bool esEstudiantePuro = esEstudiante && !esDocente && !esAdministrador;

        // Para todo estudiante puro (no docente, no admin), aplicar las reglas de validación por titulación
        if (esEstudiantePuro)
        {
            var carrerasTituladasIds = await _context.AlumnosTitulos
                .AsNoTracking()
                .Where(at => at.IdAlumno == user.IdSigafi)
                .Join(_context.Titulos, at => at.IdTitulo, t => t.IdTitulo, (at, t) => t.IdCarrera)
                .Where(idCarrera => idCarrera.HasValue)
                .Select(idCarrera => idCarrera!.Value)
                .Distinct()
                .ToListAsync(cancellationToken);

            bool tieneMatriculaActiva = await _context.Matriculas
                .AsNoTracking()
                .AnyAsync(m => m.IdAlumno == user.IdSigafi && (m.Retirado == false || m.Retirado == null), cancellationToken);

            if (!tieneMatriculaActiva)
            {
                throw new UnauthorizedAccessException("Estimado estudiante, no registra una matrícula activa en la institución para el periodo académico vigente. Contacte a Secretaría Académica.");
            }

            if (carrerasTituladasIds.Count > 0)
            {
                var carrerasAlumno = await _context.AlumnosCarreras
                    .AsNoTracking()
                    .Where(ac => ac.IdAlumno == user.IdSigafi)
                    .Select(ac => ac.IdCarrera)
                    .ToListAsync(cancellationToken);

                bool tieneNuevaCarreraPendiente = carrerasAlumno.Any(idC => !carrerasTituladasIds.Contains(idC));

                if (!tieneNuevaCarreraPendiente)
                {
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

        var refreshTokenEntity = new RbacRefreshTokens
        {
            IdUsuario = user.IdUsuario,
            TokenHash = refreshTokenHash,
            DeviceInfo = deviceInfo,
            IpAddress = ipAddress,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            FamilyId = Guid.NewGuid().ToString()
        };

        _context.RbacRefreshTokens.Add(refreshTokenEntity);
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

        var tokenEntity = await _context.RbacRefreshTokens
            .Include(t => t.IdUsuarioNavigation)
            .FirstOrDefaultAsync(t => t.TokenHash == tokenHash, cancellationToken);

        if (tokenEntity == null || tokenEntity.RevokedAt != null || tokenEntity.ExpiresAt <= DateTime.UtcNow)
        {
            throw new UnauthorizedAccessException("El refresh token es inválido o ha expirado.");
        }

        var user = tokenEntity.IdUsuarioNavigation;
        if (user.Activo != true)
        {
            throw new UnauthorizedAccessException("El usuario se encuentra inactivo.");
        }

        tokenEntity.RevokedAt = DateTime.UtcNow;
        tokenEntity.RevokedReason = "Reemplazado por nuevo token (rotation)";

        var permissions = await _rbacService.BuildUserPermissionsAsync(user.IdUsuario, "TITULACION", cancellationToken);
        var allPermissionsList = permissions.Modulos
            .SelectMany(m => m.Operaciones.Select(o => $"{m.NombreModulo}:{o}"))
            .ToList();

        var (newAccessToken, expiresAt) = _jwtTokenGenerator.GenerateAccessToken(user, permissions.Roles, allPermissionsList);
        var newRefreshTokenValue = _jwtTokenGenerator.GenerateRefreshToken();
        var newRefreshTokenHash = _jwtTokenGenerator.HashToken(newRefreshTokenValue);

        var newRefreshTokenEntity = new RbacRefreshTokens
        {
            IdUsuario = user.IdUsuario,
            TokenHash = newRefreshTokenHash,
            DeviceInfo = deviceInfo,
            IpAddress = ipAddress,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            FamilyId = tokenEntity.FamilyId
        };

        _context.RbacRefreshTokens.Add(newRefreshTokenEntity);
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
        var tokenEntity = await _context.RbacRefreshTokens
            .FirstOrDefaultAsync(t => t.TokenHash == tokenHash, cancellationToken);

        if (tokenEntity == null || tokenEntity.RevokedAt != null)
        {
            return false;
        }

        var safeReason = string.IsNullOrWhiteSpace(reason)
            ? "Cierre de sesión"
            : (reason.Length > 30 ? reason[..30] : reason);

        tokenEntity.RevokedAt = DateTime.UtcNow;
        tokenEntity.RevokedReason = safeReason;

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<UserPermissionsDto> GetUserPermissionsAsync(int idUsuario, string systemCode = "TITULACION", CancellationToken cancellationToken = default)
    {
        return await _rbacService.BuildUserPermissionsAsync(idUsuario, systemCode, cancellationToken);
    }
}
