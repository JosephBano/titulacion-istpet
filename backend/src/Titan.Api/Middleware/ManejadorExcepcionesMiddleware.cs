using System.Net;
using Microsoft.AspNetCore.Mvc;

namespace Titan.Api.Middleware;

/// <summary>
/// Único punto donde una excepción se convierte en respuesta HTTP RFC 7807 (problem+json).
/// Los controladores no atrapan excepciones; dejan que burbujeen hasta aquí.
/// </summary>
public class ManejadorExcepcionesMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ManejadorExcepcionesMiddleware> _logger;
    private readonly IHostEnvironment _env;

    public ManejadorExcepcionesMiddleware(
        RequestDelegate next, ILogger<ManejadorExcepcionesMiddleware> logger, IHostEnvironment env)
    {
        _next = next;
        _logger = logger;
        _env = env;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await EscribirRespuesta(context, ex);
        }
    }

    private async Task EscribirRespuesta(HttpContext context, Exception ex)
    {
        var problema = ex switch
        {
            KeyNotFoundException n => new ProblemDetails
            {
                Status = (int)HttpStatusCode.NotFound,
                Title = "Recurso no encontrado.",
                Detail = n.Message
            },
            InvalidOperationException i => new ProblemDetails
            {
                Status = (int)HttpStatusCode.BadRequest,
                Title = "Operación no válida.",
                Detail = i.Message
            },
            UnauthorizedAccessException u => new ProblemDetails
            {
                Status = (int)HttpStatusCode.Unauthorized,
                Title = "Acceso no autorizado.",
                Detail = u.Message
            },
            _ => new ProblemDetails
            {
                Status = (int)HttpStatusCode.InternalServerError,
                Title = "Error interno del servidor.",
                Detail = _env.IsDevelopment() ? ex.ToString() : null
            }
        };

        var ruta = context.Request.Path.ToString();
        if (problema.Status >= 500)
        {
            LogsMiddleware.ErrorNoControlado(_logger, ruta, ex);
        }
        else
        {
            LogsMiddleware.SolicitudRechazada(_logger, ruta, ex.Message);
        }

        problema.Instance = context.Request.Path;
        context.Response.Clear();
        context.Response.StatusCode = problema.Status!.Value;
        context.Response.ContentType = "application/problem+json";
        await context.Response.WriteAsJsonAsync(problema, problema.GetType());
    }
}
