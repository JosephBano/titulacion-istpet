---
name: titan-backend
description: Parámetros, reglas y estándares de arquitectura backend en C# / .NET 8 (Clean Architecture, Controllers REST, DTOs, EF Core MySQL y JWT).
---

# Estándar de Arquitectura Backend — Titán ISTPET (.NET 8)

Este documento define las reglas de arquitectura, convencion de codigo, seguridad y calidad para el backend del sistema **Titan** del Instituto Tecnologico Superior Traversari (ISTPET). Toda implementacion debe cumplir estos estandares sin excepcion.

---

## 1. Mentalidad de Arquitecto Senior — Reglas de Oro

El agente actúa como un **Ingeniero Principal y Arquitecto de Software Senior con 10+ años de experiencia en sistemas de producción enterprise**.

- **Principio de Responsabilidad Única (SRP) y Evitar "God Class"**:
  - **Controllers / Endpoints**: Máximo ~100-150 líneas (ideal: 30 a 80 líneas). Solo validan la entrada HTTP, delegan la ejecución y retornan Status Code + DTO.
  - **Servicios / Casos de Uso (Application Layer)**: Máximo ~150-200 líneas (ideal: 50 a 120 líneas). Si excede 200 líneas o requiere más de 4-5 dependencias, refactorizar aplicando CQRS / Handlers.
  - **Entidades de Dominio**: Máximo ~100-150 líneas. Contienen reglas de negocio y eventos de dominio; cero lógica de persistencia o acceso a BD.
  - **DTOs / Mappings**: Máximo ~30-50 líneas. Usar `record` posicionales para reducir a 3-5 líneas.
- **Sustituir "Servicios Gigantes" por Casos de Uso (CQRS / REPR / Handlers)**:
  - Preferir la división por Casos de Uso (*One Handler per File*). En lugar de servicios monolíticos (`UserService` con 20 métodos), usar `Commands` (modificaciones) y `Queries` (lecturas) con `IRequestHandler` o Minimal APIs / REPR pattern.
- **Inyección de Dependencias Limpia**:
  - Si un constructor supera 4 o 5 dependencias, la clase está asumiendo demasiadas responsabilidades. Delegar en servicios especializados o Domain Events.
- **Aprovechar C# Moderno (.NET 8/9)**:
  - Usar **Primary Constructors** para eliminar boilerplate de campos privados.
  - Usar **Records** posicionales para DTOs e inmutabilidad automática.
  - Usar **File-scoped Namespaces** (`namespace Titan.Domain.Entities;`) y **Global Usings**.
- **Causa raíz, no síntomas**: Todo bug o deficiencia se resuelve atacando su origen arquitectónico. Prohibido aplicar workarounds, parches silenciosos o capas de corrección superficial.
- **Rediseño sobre remiendo**: Si un problema viene de un tipo mal definido, una entidad mal mapeada o un contrato incorrecto, se propone el rediseño correcto — no se agrega código defensivo encima.
- **Cuestionar antes de implementar**: Si la petición del usuario tiene un defecto de diseño, se notifica proactivamente con la solución correcta antes de escribir código.
- **Cero `any`, cero `dynamic`, cero `object` sin justificación**: Todo dato tiene un tipo explícito y documentado.
- **Cero métodos síncronos en operaciones I/O**: Toda lectura/escritura a base de datos, archivos o servicios externos es `async/await` con `CancellationToken`.

---

## 2. Estructura de Capas — Clean Architecture

```
backend/src/
├── Titan.Domain/           <- Nucleo. Sin dependencias externas.
│   ├── Entities/           <- Entidades mapeadas de MySQL (EF Core)
│   ├── Interfaces/
│   │   ├── Security/       <- IPasswordHasher, IJwtTokenGenerator
│   │   └── Repositories/   <- Contratos de repositorios (opcional)
│   └── Enums/              <- Enumeraciones de dominio
│
├── Titan.Application/      <- Casos de uso. Solo depende de Domain.
│   ├── DTOs/
│   │   ├── Auth/           <- LoginRequestDto, LoginResponseDto, RefreshTokenRequestDto
│   │   ├── Users/          <- UserPermissionsDto, UserProfileDto
│   │   └── [Modulo]/       <- DTOs por modulo de negocio
│   ├── Interfaces/         <- IAuthService, IRbacService, IRbacManagementService
│   └── Validators/         <- Validaciones de DTOs (FluentValidation)
│
├── Titan.Infrastructure/   <- Implementaciones tecnicas. Depende de Application.
│   ├── Data/
│   │   └── TitanDbContext.cs
│   ├── Security/
│   │   ├── PasswordHasher.cs
│   │   └── JwtTokenGenerator.cs
│   ├── Services/           <- AuthService, RbacService, RbacManagementService
│   └── DependencyInjection.cs
│
└── Titan.Api/              <- Presentacion. Depende de Application e Infrastructure.
    ├── Controllers/        <- AuthController, RbacController, [Modulo]Controller
    ├── Attributes/         <- HasPermissionAttribute
    ├── Middleware/          <- Middleware de manejo de errores global
    ├── Extensions/         <- Extensiones de IServiceCollection
    ├── Program.cs
    └── appsettings.json
```

**Regla de dependencias**: Las flechas van hacia adentro. `Api` -> `Application` -> `Domain`. `Infrastructure` implementa `Application`. Nunca al reves.

---

## 3. Convencion de Nombres

### Controladores
- Nombre: `[Modulo]Controller` — PascalCase, sufijo `Controller`.
- Ruta base: `[controller]` en el atributo `[Route("api/[controller]")]`.
- Ejemplos: `AuthController`, `TitulacionController`, `GraduadosController`.

### DTOs
- Sufijos obligatorios segun proposito:
  - `[Recurso]RequestDto` — datos de entrada de un endpoint.
  - `[Recurso]ResponseDto` — datos de salida de un endpoint.
  - `[Recurso]Dto` — objeto de transferencia sin contexto de request/response.
- Ejemplos: `LoginRequestDto`, `LoginResponseDto`, `UserPermissionsDto`.
- Los DTOs son clases inmutables con propiedades `init` o registros `record`.

### Servicios e Interfaces
- Interfaz: `I[Nombre]Service` en `Titan.Application/Interfaces/`.
- Implementacion: `[Nombre]Service` en `Titan.Infrastructure/Services/`.
- Ejemplos: `IAuthService` / `AuthService`, `IRbacService` / `RbacService`.

### Entidades
- PascalCase, singular, sin sufijo: `Usuario`, `Graduado`, `ActaDeGrado`.
- Propiedades en PascalCase: `PrimerNombre`, `FechaTitulacion`, `EstadoAcademico`.

---

## 4. Endpoints REST — Convencion Semantica

| Operacion | Verbo HTTP | Ruta | Codigo exito |
|---|---|---|---|
| Listar recursos | `GET` | `/api/titulaciones` | `200 OK` |
| Obtener por ID | `GET` | `/api/titulaciones/{id}` | `200 OK` |
| Crear | `POST` | `/api/titulaciones` | `201 Created` |
| Actualizar completo | `PUT` | `/api/titulaciones/{id}` | `200 OK` |
| Actualizar parcial | `PATCH` | `/api/titulaciones/{id}` | `200 OK` |
| Eliminar | `DELETE` | `/api/titulaciones/{id}` | `204 No Content` |
| Accion especifica | `POST` | `/api/titulaciones/{id}/aprobar` | `200 OK` |

- Rutas en **kebab-case en espanol**: `/api/actas-de-grado`, no `/api/actasGrado` ni `/api/ActasDeGrado`.
- Versioning: cuando se requiera, prefijo `/api/v1/`.

---

## 5. Manejo de Errores — ProblemDetails RFC 7807

Todas las respuestas de error usan el estandar `ProblemDetails` de ASP.NET Core. Prohibido retornar strings de error planos o estructuras de error caseras.

```csharp
// Registro en Program.cs
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

// Ejemplo de excepcion de dominio
public class NotFoundException : Exception
{
    public NotFoundException(string resource, object id)
        : base($"El recurso '{resource}' con identificador '{id}' no fue encontrado.") { }
}

// GlobalExceptionHandler mapea excepciones a ProblemDetails
// NotFoundException        -> 404 Not Found
// ValidationException      -> 422 Unprocessable Entity
// UnauthorizedAccessException -> 401 Unauthorized
// Exception (general)      -> 500 Internal Server Error
```

Estructura de respuesta de error estandar:
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.4",
  "title": "Recurso no encontrado",
  "status": 404,
  "detail": "El Graduado con identificador '42' no fue encontrado.",
  "traceId": "00-abc123..."
}
```

---

## 6. DTOs — Patrones de Diseno

### Inmutabilidad con Records
```csharp
// Request DTO — inmutable, validable con FluentValidation
public sealed record LoginRequestDto(
    [Required] string UsernameOrEmail,
    [Required] string Password,
    [Required] string SystemCode
);

// Response DTO — datos de salida explicitamente definidos
public sealed record LoginResponseDto(
    string AccessToken,
    string RefreshToken,
    int ExpiresIn,
    string TokenType = "Bearer"
);
```

### Nunca exponer entidades de EF Core
```csharp
// MAL — expone la entidad directamente
[HttpGet("{id}")]
public async Task<Usuario> GetUsuario(int id) { ... }   // PROHIBIDO

// BIEN — retorna DTO
[HttpGet("{id}")]
public async Task<ActionResult<UsuarioResponseDto>> GetUsuario(int id) { ... }
```

---

## 7. EF Core — Convenciones de Acceso a Datos

- `TitanDbContext` vive en `Titan.Infrastructure/Data/`.
- Todos los metodos de acceso a datos son `async` con `CancellationToken`.
- No usar `Include()` en cadena sin un proposito definido — cada consulta carga solo lo que necesita.
- Proyectar a DTO directamente en la consulta con `.Select()` cuando sea posible — no cargar la entidad completa para luego mapearla.

```csharp
// BIEN — proyeccion directa
var dto = await _context.Graduados
    .Where(g => g.Id == id)
    .Select(g => new GraduadoResponseDto(g.NombreCompleto, g.FechaTitulacion))
    .FirstOrDefaultAsync(cancellationToken);

// MAL — carga entidad completa solo para mapear
var entidad = await _context.Graduados.FindAsync(id);
var dto = new GraduadoResponseDto(entidad.NombreCompleto, entidad.FechaTitulacion);
```

- Prohibido `SaveChanges()` sincrono — siempre `SaveChangesAsync(cancellationToken)`.
- Las transacciones explicitas (`BeginTransactionAsync`) solo cuando multiples operaciones deben ser atomicas.

---

## 8. Seguridad — JWT y RBAC

### JWT Bearer
```csharp
// Configuracion en Program.cs
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ClockSkew = TimeSpan.Zero   // Sin tolerancia de tiempo — critico para refresh tokens
        };
    });
```

### Refresh Token Rotation
- Los refresh tokens se almacenan hasheados en la tabla `rbac_refresh_tokens`.
- Cada uso del refresh token invalida el anterior y emite uno nuevo.
- `ClockSkew = TimeSpan.Zero` — obligatorio para evitar ventanas de vulnerabilidad.

### Atributo HasPermission
```csharp
[HasPermission("titulacion", "aprobar")]
[HttpPost("{id}/aprobar")]
public async Task<IActionResult> AprobarTitulacion(int id) { ... }
```

### CORS
- En desarrollo: permitir `localhost:4200`.
- En produccion: lista blanca explicita de origenes. Prohibido `AllowAnyOrigin()` en produccion.

---

## 9. Configuracion — appsettings y Entornos

Estructura de `appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=...;Database=sigafi_es;..."
  },
  "JwtSettings": {
    "SecretKey": "",
    "Issuer": "titan-api",
    "Audience": "titan-app",
    "AccessTokenExpirationMinutes": 60,
    "RefreshTokenExpirationDays": 7
  },
  "AllowedOrigins": ["http://localhost:4200"]
}
```

- Secretos sensibles (`SecretKey`, contrasenas de BD) van en `appsettings.Development.json` o en **User Secrets** (`dotnet user-secrets`) — nunca en `appsettings.json` commiteado.
- `appsettings.json` se versiona. `appsettings.Development.json` va en `.gitignore`.
- Leer configuracion siempre con el patron `IOptions<T>` — nunca `IConfiguration["clave"]` directamente en servicios.

---

## 10. Patrones de Respuesta en Controladores

```csharp
// Patron estandar de un controlador
[ApiController]
[Route("api/[controller]")]
public class TitulacionController : ControllerBase
{
    private readonly ITitulacionService _service;

    public TitulacionController(ITitulacionService service)
        => _service = service;

    [HttpGet]
    [HasPermission("titulacion", "leer")]
    public async Task<ActionResult<IEnumerable<TitulacionResponseDto>>> GetAll(
        CancellationToken cancellationToken)
    {
        var result = await _service.GetAllAsync(cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    [HasPermission("titulacion", "crear")]
    public async Task<ActionResult<TitulacionResponseDto>> Create(
        [FromBody] TitulacionRequestDto request,
        CancellationToken cancellationToken)
    {
        var created = await _service.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }
}
```

- Siempre retornar `ActionResult<T>` — no `IActionResult` plano en endpoints con datos.
- Siempre recibir `CancellationToken` en el parametro del metodo.
- Nunca capturar excepciones en el controlador — el `GlobalExceptionHandler` las maneja.

---

## 11. Anti-patrones Prohibidos

| `God Class` / Servicios gigantes (>200 líneas) | Dividir en Casos de Uso / Handlers individuales (CQRS / REPR) |
| Controladores masivos con lógica (>100-150 líneas) | Solo validar request, delegar ejecución y retornar Status Code + DTO |
| Constructores inflados con > 4-5 dependencias | Dividir responsabilidades o usar Domain Events / subservicios |
| Entidades con lógica de persistencia o acceso a BD | Entidades puras con reglas de negocio y eventos de dominio |
| DTOs verbosos con campos y getters/setters tradicionales | Usar `record` posicionales inmutables de C# |
| Lógica de negocio en controladores | Lógica en servicios / Handlers de Application |
| Entidades de EF Core en respuestas de API | Solo DTOs en respuestas |
| `SaveChanges()` síncrono | `SaveChangesAsync(cancellationToken)` |
| `IConfiguration["clave"]` en servicios | `IOptions<T>` tipado |
| `AllowAnyOrigin()` en producción | Lista blanca de orígenes |
| Strings de error planos en respuestas | `ProblemDetails` estándar |
| `ClockSkew` > 0 en JWT | `ClockSkew = TimeSpan.Zero` |
| `dynamic` u `object` sin justificación | Tipos explícitos y DTOs tipados |
| Rutas en PascalCase o camelCase | kebab-case en español |
| Métodos síncronos en I/O | `async/await` con `CancellationToken` |
| Capturar excepciones en controladores | Manejo global en `GlobalExceptionHandler` |
| `Include()` sin propósito definido | Proyección directa con `.Select()` |

---

## 12. Referencias Internas del Proyecto

- [Program.cs](file:///c:/Users/DESARROLLADOR/Downloads/titan/backend/src/Titan.Api/Program.cs)
- [AuthController.cs](file:///c:/Users/DESARROLLADOR/Downloads/titan/backend/src/Titan.Api/Controllers/AuthController.cs)
- [DependencyInjection.cs](file:///c:/Users/DESARROLLADOR/Downloads/titan/backend/src/Titan.Infrastructure/DependencyInjection.cs)
- [TitanDbContext.cs](file:///c:/Users/DESARROLLADOR/Downloads/titan/backend/src/Titan.Infrastructure/Data/TitanDbContext.cs)
