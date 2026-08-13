using System.Net;
using Microsoft.AspNetCore.Mvc;
using TitulacionIstpet.Application.Common.Models;
using TitulacionIstpet.Domain.Exceptions;

namespace TitulacionIstpet.WebApi.Middleware;

/// <summary>
/// Unico punto donde una excepcion se convierte en respuesta HTTP. Los controllers
/// no atrapan excepciones; dejan que burbujeen hasta aqui.
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
            ValidacionException v => new ValidationProblemDetails(v.Errores)
            {
                Status = (int)HttpStatusCode.BadRequest,
                Title = "Error de validacion."
            },
            NoEncontradoException n => new ProblemDetails
            {
                Status = (int)HttpStatusCode.NotFound,
                Title = "Recurso no encontrado.",
                Detail = n.Message
            },
            DominioException d => new ProblemDetails
            {
                Status = (int)HttpStatusCode.Conflict,
                Title = "Regla de negocio violada.",
                Detail = d.Message
            },
            _ => new ProblemDetails
            {
                Status = (int)HttpStatusCode.InternalServerError,
                Title = "Error interno del servidor.",
                // Nunca filtrar stack traces fuera de Development.
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
