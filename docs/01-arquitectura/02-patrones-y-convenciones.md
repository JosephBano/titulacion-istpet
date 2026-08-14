# Patrones de Diseño y Convenciones de Código — Sistema Titulación ISTPET

## 1. Patrones de Diseño Implementados

### 1.1. Asincronismo I/O con CancellationTokens
Toda consulta a la base de datos, verificación de tokens o procesamiento de servicios es estrictamente asíncrono (`async/await`) y requiere un `CancellationToken` propuesto desde la API hasta la infraestructura para permitir la cancelación de peticiones abandonadas.

```csharp
public async Task<IEnumerable<CarreraResponseDto>> GetCarrerasActivasAsync(CancellationToken cancellationToken)
{
    return await _context.carreras
        .AsNoTracking()
        .Where(c => c.activo == 1)
        .Select(c => new CarreraResponseDto(
            c.idCarrera,
            c.carrera,
            c.nombreCorto,
            c.activo
        ))
        .ToListAsync(cancellationToken);
}
```

---

### 1.2. Patrón Proyección Directa a DTO (Direct Projection Pattern)
Para garantizar alta eficiencia y evitar problemas de N+1 consultas o ciclos de serialización JSON, las consultas de lectura en Entity Framework Core 8 proyectan las columnas SQL directamente a DTOs de salida utilizando `.Select()`. No se cargan entidades completas cuando solo se requieren campos específicos.

---

### 1.3. DTOs Inmutables con Records de C#
Los objetos de transferencia de datos de entrada y salida se definen como registros inmutables (`record` o `sealed record`).

```csharp
public sealed record LoginRequestDto(
    string UsernameOrEmail,
    string Password,
    string SystemCode = "TITULACION"
);

public sealed record CarreraResponseDto(
    int IdCarrera,
    string NombreCarrera,
    string NombreCorto,
    sbyte? Activo
);
```

---

### 1.4. Inyección de Dependencias Descentralizada
El registro de servicios de infraestructura y acceso a datos se encapsula en la clase de extensión `DependencyInjection.cs` de `TitulacionIstpet.Infrastructure`:

```csharp
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        
        services.AddDbContext<SigafiDbContext>(options =>
            options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IRbacService, RbacService>();
        services.AddScoped<IRbacManagementService, RbacManagementService>();
        services.AddScoped<IAcademicoService, AcademicoService>();
        services.AddScoped<IActoresService, ActoresService>();
        
        return services;
    }
}
```

---

## 2. Convenciones de Nombres y Código

- **Controladores:** PascalCase con sufijo `Controller` (`AuthController`, `AcademicoController`).
- **Interfaces:** Prefijo `I` en PascalCase (`IAcademicoService`, `IActoresService`).
- **Servicios:** Sufijo `Service` (`AcademicoService`, `ActoresService`).
- **DTOs:** Sufijos explícitos segun propósito (`[Nombre]RequestDto`, `[Nombre]ResponseDto`).
- **Rutas API:** Prefijo `/api/v1/` seguido del nombre del controlador en kebab-case (`/api/v1/academico/carreras`).


