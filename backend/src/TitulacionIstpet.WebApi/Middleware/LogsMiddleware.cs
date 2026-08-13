namespace TitulacionIstpet.WebApi.Middleware;

/// <summary>
/// Mensajes de log generados en tiempo de compilacion. El analizador CA1848 exige
/// este patron en vez de ILogger.LogX directo: evita el boxing de los argumentos
/// cuando el nivel esta desactivado. Copiar esta forma al agregar logs nuevos.
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
