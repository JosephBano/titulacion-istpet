using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using TitulacionIstpet.Application.Common.Interfaces;
using TitulacionIstpet.WebApi.Services;

namespace TitulacionIstpet.WebApi.Extensions;

public static class WebApiServiceCollectionExtensions
{
    public const string PoliticaCorsFrontend = "FrontendAngular";

    public static IServiceCollection AddWebApi(
        this IServiceCollection services, IConfiguration configuration)
    {
        services.AddControllers();
        services.AddHttpContextAccessor();
        services.AddScoped<IUsuarioActual, UsuarioActual>();

        // 1. Configuración de Autenticación JWT Bearer
        var secretKey = configuration["JwtSettings:SecretKey"] ?? "TitulacionIstpetSystemSecretKeyForJwtAuthenticationSuperSecure2026!";
        var issuer = configuration["JwtSettings:Issuer"] ?? "TitulacionIstpetApi";
        var audience = configuration["JwtSettings:Audience"] ?? "TitulacionIstpetApp";

        services.AddAuthentication(options =>
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

        // 2. Swagger con soporte Bearer
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo { Title = "Titulación ISTPET API", Version = "v1" });

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

        // 3. CORS
        string[] origenes = configuration.GetSection("Cors:OrigenesPermitidos").Get<string[]>()
            ?? ["http://localhost:4200", "https://localhost:4200"];

        services.AddCors(options => options.AddPolicy(PoliticaCorsFrontend, policy =>
            policy.WithOrigins(origenes).AllowAnyHeader().AllowAnyMethod().AllowCredentials()));

        return services;
    }
}
