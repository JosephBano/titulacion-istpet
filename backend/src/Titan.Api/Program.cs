using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Titan.Infrastructure;
using Titan.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

// Aumentar límites de Kestrel para evitar HTTP 431 en peticiones con encabezados o tokens largos
builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestHeadersTotalSize = 128 * 1024; // 128 KB
    options.Limits.MaxRequestLineSize = 64 * 1024; // 64 KB
});

// 1. Configuración de Base de Datos EF Core MySQL
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
Console.WriteLine($"\n[DB-DEBUG] Cadena de Conexión en Titan.Api: '{connectionString}'\n");
builder.Services.AddDbContext<TitanDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

// 2. Inyección de Servicios de Infraestructura y Aplicación (Auth, RBAC, Hasher, JWT)
builder.Services.AddInfrastructureServices(builder.Configuration);

// 3. Controladores API REST
builder.Services.AddControllers();

// 4. Configuración de Autenticación JWT Bearer
var secretKey = builder.Configuration["JwtSettings:SecretKey"] ?? "TitanSystemSecretKeyForJwtAuthenticationSuperSecure2026!";
var issuer = builder.Configuration["JwtSettings:Issuer"] ?? "TitanApi";
var audience = builder.Configuration["JwtSettings:Audience"] ?? "TitanApp";

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = issuer,
        ValidAudience = audience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
        ClockSkew = TimeSpan.Zero
    };
});

// 5. Configuración de CORS para cliente Angular
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularClient", policy =>
    {
        policy.WithOrigins("http://localhost:4200", "https://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// 6. Swagger OpenAPI con soporte para Token JWT Bearer
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Titan API - Sistema de Titulación", Version = "v1" });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "Autenticación JWT usando el esquema Bearer. Ejemplo: 'Bearer {token}'",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// Aplicar CORS al inicio del pipeline antes de cualquier middleware que pueda retornar errores o redirigir
app.UseCors("AllowAngularClient");

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Titan API v1"));
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();




using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<TitanDbContext>();
    var idTarget = "0103563086";
    var esAlumno = db.alumnos.Any(a => a.idAlumno == idTarget);
    var titulos = db.alumnos_titulos.Where(a => a.idAlumno == idTarget).ToList();
    var matriculas = db.matriculas.Where(m => m.idAlumno == idTarget).ToList();
    var carreras = db.alumnos_carreras.Where(ac => ac.idAlumno == idTarget).ToList();
    var prof = db.profesores.FirstOrDefault(p => p.idProfesor == idTarget);

    Console.WriteLine($"\n==================== [DIAGNOSTICO TERESA PONCE ({idTarget})] ====================");
    Console.WriteLine($"-> Existe en 'alumnos': {esAlumno}");
    Console.WriteLine($"-> Profesores -> Activo: {prof?.activo} | FechaRetiro: {prof?.fecha_retiro}");
    Console.WriteLine($"-> Total en 'alumnos_titulos': {titulos.Count}");
    foreach (var t in titulos)
    {
        Console.WriteLine($"   * idTitulo: {t.idTitulo}");
    }

    Console.WriteLine($"-> Total en 'matriculas': {matriculas.Count}");
    foreach (var m in matriculas)
    {
        Console.WriteLine($"   * idMatricula: {m.idMatricula} | Retirado: {m.retirado}");
    }

    Console.WriteLine($"-> Total en 'alumnos_carreras': {carreras.Count}");
    foreach (var c in carreras)
    {
        Console.WriteLine($"   * idCarrera: {c.idCarrera}");
    }

    Console.WriteLine("========================================================================================\n");
}

app.Run();
