namespace Titan.Api.Middleware;

/// <summary>
/// Mensajes de log generados en tiempo de compilación. El analizador CA1848 exige
/// este patrón en vez de ILogger.LogX directo: evita el boxing de los argumentos
/// cuando el nivel está desactivado.
/// </summary>
internal static partial class LogsMiddleware
{
    [LoggerMessage(
        EventId = 1000,
        Level = LogLevel.Error,
        Message = "Error no controlado en {Ruta}")]
    public static partial void ErrorNoControlado(ILogger logger, string ruta, Exception excepcion);

    [LoggerMessage(
        EventId = 1001,
        Level = LogLevel.Warning,
        Message = "Solicitud rechazada en {Ruta}: {Mensaje}")]
    public static partial void SolicitudRechazada(ILogger logger, string ruta, string mensaje);
}
