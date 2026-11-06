using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
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
    private readonly IConfiguration _configuration;

    public AuthService(
        SigafiDbContext context,
        IPasswordHasher passwordHasher,
        IJwtTokenGenerator jwtTokenGenerator,
        IRbacService rbacService,
        IConfiguration configuration)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _jwtTokenGenerator = jwtTokenGenerator;
        _rbacService = rbacService;
        _configuration = configuration;
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

    public async Task<ValidarDocumentoRecuperacionResponseDto> ValidarDocumentoRecuperacionAsync(ValidarDocumentoRecuperacionRequestDto request, CancellationToken cancellationToken = default)
    {
        var cleanInput = request.NumeroDocumento?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(cleanInput))
        {
            return new ValidarDocumentoRecuperacionResponseDto
            {
                Valido = false,
                Mensaje = "Por favor, ingresa tu número de documento de identidad."
            };
        }

        var user = await _context.Usuarios
            .FirstOrDefaultAsync(u =>
                u.IdSigafi == cleanInput ||
                (u.EmailInstitucional != null && u.EmailInstitucional == cleanInput) ||
                (u.Nombre != null && u.Nombre == cleanInput),
                cancellationToken);

        if (user == null)
        {
            var alumno = await _context.Alumnos
                .FirstOrDefaultAsync(a =>
                    a.IdAlumno == cleanInput ||
                    (a.UserAlumno != null && a.UserAlumno == cleanInput) ||
                    (a.EmailInstitucional != null && a.EmailInstitucional == cleanInput) ||
                    (a.Email != null && a.Email == cleanInput),
                    cancellationToken);

            if (alumno != null)
            {
                var (apto, motivo) = await VerificarAptitudEstudianteAsync(alumno.IdAlumno, cancellationToken);
                if (!apto)
                {
                    return new ValidarDocumentoRecuperacionResponseDto
                    {
                        Valido = false,
                        Mensaje = motivo
                    };
                }

                var nombre = $"{alumno.PrimerNombre} {alumno.ApellidoPaterno}".Trim();
                return new ValidarDocumentoRecuperacionResponseDto
                {
                    Valido = true,
                    NombreCompleto = string.IsNullOrWhiteSpace(nombre) ? alumno.IdAlumno : nombre,
                    Mensaje = "Documento verificado correctamente."
                };
            }

            var profesor = await _context.Profesores
                .FirstOrDefaultAsync(p =>
                    p.IdProfesor == cleanInput ||
                    (p.EmailInstitucional != null && p.EmailInstitucional == cleanInput) ||
                    (p.Email != null && p.Email == cleanInput),
                    cancellationToken);

            if (profesor != null)
            {
                if (profesor.Activo != true)
                {
                    return new ValidarDocumentoRecuperacionResponseDto
                    {
                        Valido = false,
                        Mensaje = "El docente se encuentra inactivo en la institución."
                    };
                }

                var nombreDocente = $"{profesor.PrimerNombre} {profesor.Apellidos}".Trim();
                return new ValidarDocumentoRecuperacionResponseDto
                {
                    Valido = true,
                    NombreCompleto = string.IsNullOrWhiteSpace(nombreDocente) ? profesor.IdProfesor : nombreDocente,
                    Mensaje = "Documento verificado correctamente."
                };
            }

            return new ValidarDocumentoRecuperacionResponseDto
            {
                Valido = false,
                Mensaje = "El documento de identidad ingresado no se encuentra registrado en el sistema."
            };
        }

        if (user.Activo != true)
        {
            return new ValidarDocumentoRecuperacionResponseDto
            {
                Valido = false,
                Mensaje = "El usuario se encuentra inactivo en el sistema."
            };
        }

        var permissions = await _rbacService.BuildUserPermissionsAsync(user.IdUsuario, "TITULACION", cancellationToken);
        bool esDocente = string.Equals(user.TablaSigafi, "profesor", StringComparison.OrdinalIgnoreCase) ||
            permissions.Roles.Any(r => r.Contains("DOCENTE", StringComparison.OrdinalIgnoreCase) || r.Contains("PROFESOR", StringComparison.OrdinalIgnoreCase));
        bool esAdministrador = user.Administrador == true || permissions.Roles.Any(r => r.Contains("ADMIN", StringComparison.OrdinalIgnoreCase));
        bool esEstudiante = string.Equals(user.TablaSigafi, "alumno", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(user.TablaSigafi, "alumnos", StringComparison.OrdinalIgnoreCase) ||
            await _context.Alumnos.AsNoTracking().AnyAsync(a => a.IdAlumno == user.IdSigafi, cancellationToken);

        if (!esAdministrador && !esDocente && !esEstudiante)
        {
            return new ValidarDocumentoRecuperacionResponseDto
            {
                Valido = false,
                Mensaje = "Su cuenta se encuentra inactiva por retiro o desvinculación institucional."
            };
        }

        if (esEstudiante && !esDocente && !esAdministrador)
        {
            var (apto, motivo) = await VerificarAptitudEstudianteExistenteAsync(user.IdSigafi, cancellationToken);
            if (!apto)
            {
                return new ValidarDocumentoRecuperacionResponseDto
                {
                    Valido = false,
                    Mensaje = motivo
                };
            }
        }

        return new ValidarDocumentoRecuperacionResponseDto
        {
            Valido = true,
            NombreCompleto = user.Nombre,
            Mensaje = "Documento verificado correctamente."
        };
    }

    public async Task<ValidarFechaNacimientoResponseDto> ValidarFechaNacimientoAsync(ValidarFechaNacimientoRequestDto request, CancellationToken cancellationToken = default)
    {
        var cleanInput = request.NumeroDocumento?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(cleanInput))
        {
            return new ValidarFechaNacimientoResponseDto
            {
                Valido = false,
                Mensaje = "Número de documento no proporcionado."
            };
        }

        DateOnly? fechaRegistrada = null;

        var alumno = await _context.Alumnos
            .AsNoTracking()
            .FirstOrDefaultAsync(a =>
                a.IdAlumno == cleanInput ||
                (a.UserAlumno != null && a.UserAlumno == cleanInput) ||
                (a.EmailInstitucional != null && a.EmailInstitucional == cleanInput) ||
                (a.Email != null && a.Email == cleanInput),
                cancellationToken);

        if (alumno != null)
        {
            fechaRegistrada = alumno.FechaNacimiento;
        }
        else
        {
            var profesor = await _context.Profesores
                .AsNoTracking()
                .FirstOrDefaultAsync(p =>
                    p.IdProfesor == cleanInput ||
                    (p.EmailInstitucional != null && p.EmailInstitucional == cleanInput) ||
                    (p.Email != null && p.Email == cleanInput),
                    cancellationToken);

            if (profesor != null)
            {
                fechaRegistrada = profesor.FechaNacimiento;
            }
            else
            {
                var user = await _context.Usuarios
                    .AsNoTracking()
                    .FirstOrDefaultAsync(u =>
                        u.IdSigafi == cleanInput ||
                        (u.EmailInstitucional != null && u.EmailInstitucional == cleanInput),
                        cancellationToken);

                if (user != null)
                {
                    if (string.Equals(user.TablaSigafi, "profesor", StringComparison.OrdinalIgnoreCase))
                    {
                        var prof = await _context.Profesores
                            .AsNoTracking()
                            .FirstOrDefaultAsync(p => p.IdProfesor == user.IdSigafi, cancellationToken);
                        fechaRegistrada = prof?.FechaNacimiento;
                    }
                    else
                    {
                        var alum = await _context.Alumnos
                            .AsNoTracking()
                            .FirstOrDefaultAsync(a => a.IdAlumno == user.IdSigafi, cancellationToken);
                        fechaRegistrada = alum?.FechaNacimiento;
                    }
                }
            }
        }

        if (fechaRegistrada == null)
        {
            return new ValidarFechaNacimientoResponseDto
            {
                Valido = false,
                Mensaje = "No se encuentra registrada una fecha de nacimiento institucional para validar. Por favor, acérquese a Secretaría Académica."
            };
        }

        if (fechaRegistrada.Value != request.FechaNacimiento)
        {
            return new ValidarFechaNacimientoResponseDto
            {
                Valido = false,
                Mensaje = "La fecha de nacimiento no coincide con los registros institucionales."
            };
        }

        return new ValidarFechaNacimientoResponseDto
        {
            Valido = true,
            Mensaje = "Identidad validada exitosamente.",
            Token = GenerateRecoveryToken(cleanInput)
        };
    }

    private async Task<(bool apto, string mensaje)> VerificarAptitudEstudianteAsync(string idAlumno, CancellationToken cancellationToken)
    {
        var matriculasAlumno = await _context.Matriculas
            .AsNoTracking()
            .Include(m => m.IdNivelNavigation)
            .Where(m => m.IdAlumno == idAlumno && (m.Retirado == null || m.Retirado == false))
            .ToListAsync(cancellationToken);

        if (matriculasAlumno.Count == 0)
        {
            return (false, "Estimado estudiante, no registra matrículas activas en la institución.");
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
                .AnyAsync(p => p.IdMatriculaNavigation.IdAlumno == idAlumno, cancellationToken);

            if (tienePostulacionPrevia)
            {
                esAptoTitulacion = true;
            }
        }

        if (!esAptoTitulacion)
        {
            var nivelMsg = !string.IsNullOrWhiteSpace(nombreNivelActual) ? $" (Nivel registrado: {nombreNivelActual})" : string.Empty;
            return (false, $"Estimado estudiante, el Sistema de Titulación está habilitado exclusivamente para estudiantes del último semestre o egresados{nivelMsg}.");
        }

        return (true, string.Empty);
    }

    private async Task<(bool apto, string mensaje)> VerificarAptitudEstudianteExistenteAsync(string idSigafi, CancellationToken cancellationToken)
    {
        bool tieneMatriculaActiva = await _context.Matriculas
            .AsNoTracking()
            .AnyAsync(m => m.IdAlumno == idSigafi && (m.Retirado == false || m.Retirado == null), cancellationToken);

        if (!tieneMatriculaActiva)
        {
            return (false, "Estimado estudiante, no registra una matrícula activa en la institución para el periodo académico vigente. Contacte a Secretaría Académica.");
        }

        var carrerasTituladasIds = await _context.AlumnosTitulos
            .AsNoTracking()
            .Where(at => at.IdAlumno == idSigafi)
            .Join(_context.Titulos, at => at.IdTitulo, t => t.IdTitulo, (at, t) => t.IdCarrera)
            .Where(idCarrera => idCarrera.HasValue)
            .Select(idCarrera => idCarrera!.Value)
            .Distinct()
            .ToListAsync(cancellationToken);

        if (carrerasTituladasIds.Count > 0)
        {
            var carrerasAlumno = await _context.AlumnosCarreras
                .AsNoTracking()
                .Where(ac => ac.IdAlumno == idSigafi)
                .Select(ac => ac.IdCarrera)
                .ToListAsync(cancellationToken);

            bool tieneNuevaCarreraPendiente = carrerasAlumno.Any(idC => !carrerasTituladasIds.Contains(idC));

            if (!tieneNuevaCarreraPendiente)
            {
                return (false, "Estimado estudiante, usted ya se encuentra titulado en su(s) carrera(s) registrada(s) y no registra matrículas activas en una nueva carrera.");
            }
        }

        return (true, string.Empty);
    }

    public async Task<RestablecerContrasenaResponseDto> RestablecerContrasenaAsync(RestablecerContrasenaRequestDto request, CancellationToken cancellationToken = default)
    {
        var cleanInput = request.NumeroDocumento?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(cleanInput))
        {
            return new RestablecerContrasenaResponseDto
            {
                Exito = false,
                Mensaje = "Número de documento no proporcionado."
            };
        }

        if (string.IsNullOrWhiteSpace(request.Token) || !ValidateRecoveryToken(cleanInput, request.Token))
        {
            return new RestablecerContrasenaResponseDto
            {
                Exito = false,
                Mensaje = "El enlace o token de verificación es inválido o ha expirado. Por favor, reinicia el proceso de recuperación."
            };
        }

        if (string.IsNullOrWhiteSpace(request.NuevaContrasenia) || string.IsNullOrWhiteSpace(request.ConfirmarContrasenia))
        {
            return new RestablecerContrasenaResponseDto
            {
                Exito = false,
                Mensaje = "Por favor, ingresa y confirma tu nueva contraseña."
            };
        }

        if (request.NuevaContrasenia != request.ConfirmarContrasenia)
        {
            return new RestablecerContrasenaResponseDto
            {
                Exito = false,
                Mensaje = "Las contraseñas no coinciden."
            };
        }

        if (request.NuevaContrasenia.Length < 6 || request.NuevaContrasenia.Length > 20)
        {
            return new RestablecerContrasenaResponseDto
            {
                Exito = false,
                Mensaje = "La nueva contraseña debe tener entre 6 y 20 caracteres."
            };
        }

        var newHashedPassword = _passwordHasher.HashPassword(request.NuevaContrasenia);
        bool actualizado = false;

        // 1. Buscar o actualizar en Usuarios
        var user = await _context.Usuarios
            .FirstOrDefaultAsync(u =>
                u.IdSigafi == cleanInput ||
                (u.EmailInstitucional != null && u.EmailInstitucional == cleanInput) ||
                (u.Nombre != null && u.Nombre == cleanInput),
                cancellationToken);

        var alumno = await _context.Alumnos
            .FirstOrDefaultAsync(a =>
                a.IdAlumno == cleanInput ||
                (a.UserAlumno != null && a.UserAlumno == cleanInput) ||
                (a.EmailInstitucional != null && a.EmailInstitucional == cleanInput) ||
                (a.Email != null && a.Email == cleanInput),
                cancellationToken);

        var profesor = await _context.Profesores
            .FirstOrDefaultAsync(p =>
                p.IdProfesor == cleanInput ||
                (p.EmailInstitucional != null && p.EmailInstitucional == cleanInput) ||
                (p.Email != null && p.Email == cleanInput),
                cancellationToken);

        if (user != null)
        {
            user.Contrasenia = newHashedPassword;
            actualizado = true;
        }
        else if (alumno != null)
        {
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
                Contrasenia = newHashedPassword,
                Activo = true,
                Administrador = false,
                EmailInstitucional = emailInst,
                EmailValidado = true
            };

            _context.Usuarios.Add(user);
            await _context.SaveChangesAsync(cancellationToken);

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
            }
            actualizado = true;
        }
        else if (profesor != null)
        {
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
                Contrasenia = newHashedPassword,
                Activo = true,
                Administrador = false,
                EmailInstitucional = emailInst,
                EmailValidado = true
            };

            _context.Usuarios.Add(user);
            await _context.SaveChangesAsync(cancellationToken);

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
            }
            actualizado = true;
        }

        // En Alumnos y Profesores (SIGAFI institucional) se almacena estrictamente en texto plano
        if (alumno != null)
        {
            alumno.Password = request.NuevaContrasenia;
        }

        if (profesor != null)
        {
            profesor.Clave = request.NuevaContrasenia;
        }

        if (!actualizado)
        {
            return new RestablecerContrasenaResponseDto
            {
                Exito = false,
                Mensaje = "No se encontró el registro del usuario para actualizar la contraseña."
            };
        }

        await _context.SaveChangesAsync(cancellationToken);

        return new RestablecerContrasenaResponseDto
        {
            Exito = true,
            Mensaje = "Contraseña restablecida exitosamente. Ya puedes iniciar sesión con tu nueva contraseña."
        };
    }

    private string GenerateRecoveryToken(string document)
    {
        var secret = _configuration["Jwt:ClaveFirma"]
            ?? _configuration["JwtSettings:SecretKey"]
            ?? "TitulacionIstpetSystemSecretKeyForJwtAuthenticationSuperSecure2026!";
        var expiresAt = DateTimeOffset.UtcNow.AddMinutes(15).ToUnixTimeSeconds();
        var payload = $"{document}:{expiresAt}";
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
        var signature = Convert.ToBase64String(hash);
        var raw = $"{payload}:{signature}";
        return Convert.ToBase64String(Encoding.UTF8.GetBytes(raw));
    }

    private bool ValidateRecoveryToken(string document, string token)
    {
        if (string.IsNullOrWhiteSpace(document) || string.IsNullOrWhiteSpace(token))
        {
            return false;
        }

        try
        {
            var raw = Encoding.UTF8.GetString(Convert.FromBase64String(token));
            var parts = raw.Split(':');
            if (parts.Length != 3)
            {
                return false;
            }

            var tokenDoc = parts[0];
            if (!long.TryParse(parts[1], out var expiresAt))
            {
                return false;
            }
            var signature = parts[2];

            if (!string.Equals(tokenDoc, document, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            if (DateTimeOffset.UtcNow.ToUnixTimeSeconds() > expiresAt)
            {
                return false;
            }

            var secret = _configuration["Jwt:ClaveFirma"]
                ?? _configuration["JwtSettings:SecretKey"]
                ?? "TitulacionIstpetSystemSecretKeyForJwtAuthenticationSuperSecure2026!";
            var payload = $"{tokenDoc}:{expiresAt}";
            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
            var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
            var expectedSignature = Convert.ToBase64String(computedHash);

            return CryptographicOperations.FixedTimeEquals(
                Encoding.UTF8.GetBytes(signature),
                Encoding.UTF8.GetBytes(expectedSignature));
        }
        catch
        {
            return false;
        }
    }
}
