using System.Text.RegularExpressions;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;
using TitulacionIstpet.Application.Common.Models;
using TitulacionIstpet.Application.Features.AdjuntosImagenes;
using TitulacionIstpet.Application.Features.AdjuntosImagenes.Comandos;
using TitulacionIstpet.Application.Features.AdjuntosImagenes.Consultas;

namespace TitulacionIstpet.WebApi.Controllers;

/// <summary>
/// CRUD y carga física de archivos para la tabla <c>adjuntos_imagenes</c>.
/// Gestiona la validación de extensiones, tamaños y saneamiento de nombres,
/// garantizando persistencia física y en base de datos sin archivos huérfanos.
/// </summary>
[ApiController]
[Route("api/adjuntos-imagenes")]
public sealed class AdjuntosImagenesController(
    ObtenerAdjuntoPorId obtener,
    ListarAdjuntos listar,
    CrearAdjunto crear,
    ActualizarAdjunto actualizar,
    EliminarAdjunto eliminar,
    ILogger<AdjuntosImagenesController> logger) : ControllerBase
{
    private const long MaximoTamanoBytes = 10 * 1024 * 1024; // 10 MB
    private static readonly HashSet<string> ExtensionesPermitidas =
        [".pdf", ".png", ".jpg", ".jpeg", ".doc", ".docx"];

    private readonly ObtenerAdjuntoPorId _obtener = obtener;
    private readonly ListarAdjuntos _listar = listar;
    private readonly CrearAdjunto _crear = crear;
    private readonly ActualizarAdjunto _actualizar = actualizar;
    private readonly EliminarAdjunto _eliminar = eliminar;
    private readonly ILogger<AdjuntosImagenesController> _logger = logger;

    [HttpGet]
    public Task<PaginaAdjuntos> Listar(
        [FromQuery] int pagina = 1,
        [FromQuery] int tamanoPagina = ListarAdjuntos.TamanoPaginaPorDefecto,
        CancellationToken ct = default)
        => _listar.EjecutarAsync(new ListarAdjuntosConsulta(pagina, tamanoPagina), ct);

    [HttpGet("{id:int}")]
    public Task<AdjuntosImageneDto> Obtener(int id, CancellationToken ct)
        => _obtener.EjecutarAsync(new ObtenerAdjuntoPorIdConsulta(id), ct);

    [HttpPost]
    public async Task<ActionResult<AdjuntosImageneDto>> Crear(
        [FromBody] CrearAdjuntoComando comando, CancellationToken ct)
    {
        int id = await _crear.EjecutarAsync(comando, ct);
        var dto = await _obtener.EjecutarAsync(new ObtenerAdjuntoPorIdConsulta(id), ct);
        return CreatedAtAction(nameof(Obtener), new { id }, dto);
    }

    [HttpPost("subir")]
    [HttpPost("upload")]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<AdjuntosImageneDto>> Subir(
        [FromForm(Name = "archivo")] IFormFile? archivo,
        [FromServices] IWebHostEnvironment env,
        CancellationToken ct)
    {
        if (archivo == null || archivo.Length == 0)
        {
            throw new ValidacionException([
                new ValidationFailure("archivo", "No se ha proporcionado un archivo válido o el archivo está vacío.")
            ]);
        }

        if (archivo.Length > MaximoTamanoBytes)
        {
            throw new ValidacionException([
                new ValidationFailure("archivo", $"El archivo excede el tamaño máximo permitido de {MaximoTamanoBytes / (1024 * 1024)} MB.")
            ]);
        }

        var rawFileName = archivo.FileName ?? string.Empty;
        var extensionWithDot = Path.GetExtension(rawFileName).ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(extensionWithDot) || !ExtensionesPermitidas.Contains(extensionWithDot))
        {
            throw new ValidacionException([
                new ValidationFailure("archivo", $"El formato '{extensionWithDot}' no está permitido. Solo se aceptan documentos PDF o imágenes ({string.Join(", ", ExtensionesPermitidas)}).")
            ]);
        }

        var webRoot = string.IsNullOrWhiteSpace(env.WebRootPath)
            ? Path.Combine(env.ContentRootPath, "wwwroot")
            : env.WebRootPath;
        var evidenciasDir = Path.Combine(webRoot, "evidencias");

        try
        {
            if (!Directory.Exists(evidenciasDir))
            {
                Directory.CreateDirectory(evidenciasDir);
            }
        }
        catch (UnauthorizedAccessException ex)
        {
            LogsAdjuntos.ErrorPermisosDirectorio(_logger, evidenciasDir, ex);
            throw new InvalidOperationException($"El servidor no cuenta con permisos de escritura en la carpeta de evidencias ({evidenciasDir}). Configure permisos de modificación para IIS_IUSRS o el usuario de la aplicación.", ex);
        }

        var originalFileName = Path.GetFileName(rawFileName);
        var baseName = Path.GetFileNameWithoutExtension(originalFileName);
        var cleanBaseName = Regex.Replace(baseName, @"[^a-zA-Z0-9_\-]", "_");
        if (cleanBaseName.Length > 40)
        {
            cleanBaseName = cleanBaseName[..40];
        }
        if (string.IsNullOrWhiteSpace(cleanBaseName))
        {
            cleanBaseName = "doc";
        }

        var extension = extensionWithDot.TrimStart('.');
        var uniqueFileName = $"{Guid.NewGuid():N}_{cleanBaseName}.{extension}";
        var physicalPath = Path.Combine(evidenciasDir, uniqueFileName);

        try
        {
            await using (var stream = new FileStream(physicalPath, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                await archivo.CopyToAsync(stream, ct);
            }
        }
        catch (UnauthorizedAccessException ex)
        {
            LogsAdjuntos.ErrorPermisoEscrituraArchivo(_logger, physicalPath, ex);
            throw new InvalidOperationException($"Permiso denegado al guardar el archivo en el servidor ({physicalPath}). Verifique los permisos del sistema de archivos.", ex);
        }
        catch (Exception ex)
        {
            LogsAdjuntos.ErrorEscrituraInesperada(_logger, physicalPath, ex);
            throw;
        }

        var rutaRelativa = $"/evidencias/{uniqueFileName}";
        var nombreCorto = originalFileName.Length > 85 ? originalFileName[..85] : originalFileName;

        var comando = new CrearAdjuntoComando(
            NombreArchivos: nombreCorto,
            Extension: extension.Length > 20 ? extension[..20] : extension,
            MimeTypes: string.IsNullOrWhiteSpace(archivo.ContentType)
                ? "application/octet-stream"
                : (archivo.ContentType.Length > 85 ? archivo.ContentType[..85] : archivo.ContentType),
            TamanioBytes: (int)Math.Min(archivo.Length, int.MaxValue),
            Ruta: rutaRelativa
        );

        int id;
        try
        {
            id = await _crear.EjecutarAsync(comando, ct);
        }
        catch (Exception ex)
        {
            LogsAdjuntos.ErrorRegistroBaseDatos(_logger, physicalPath, ex);
            if (System.IO.File.Exists(physicalPath))
            {
                try
                {
                    System.IO.File.Delete(physicalPath);
                }
                catch (Exception deleteEx)
                {
                    LogsAdjuntos.ErrorEliminarHuerfano(_logger, physicalPath, deleteEx);
                }
            }
            throw;
        }

        LogsAdjuntos.ArchivoSubidoExitosamente(_logger, id, nombreCorto, rutaRelativa);
        var dto = await _obtener.EjecutarAsync(new ObtenerAdjuntoPorIdConsulta(id), ct);
        return Ok(dto);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Actualizar(
        int id, [FromBody] ActualizarAdjuntoComando comando, CancellationToken ct)
    {
        if (comando.IdAdjuntosImagenes != id)
        {
            comando = comando with { IdAdjuntosImagenes = id };
        }

        await _actualizar.EjecutarAsync(comando, ct);
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Eliminar(int id, CancellationToken ct)
    {
        await _eliminar.EjecutarAsync(id, ct);
        return NoContent();
    }
}

internal static partial class LogsAdjuntos
{
    [LoggerMessage(EventId = 2001, Level = LogLevel.Error, Message = "Error de permisos al crear directorio de evidencias en {EvidenciasDir}")]
    public static partial void ErrorPermisosDirectorio(ILogger logger, string evidenciasDir, Exception excepcion);

    [LoggerMessage(EventId = 2002, Level = LogLevel.Error, Message = "Permiso denegado al escribir el archivo adjunto en {PhysicalPath}")]
    public static partial void ErrorPermisoEscrituraArchivo(ILogger logger, string physicalPath, Exception excepcion);

    [LoggerMessage(EventId = 2003, Level = LogLevel.Error, Message = "Error de E/S inesperado al guardar el archivo físico en {PhysicalPath}")]
    public static partial void ErrorEscrituraInesperada(ILogger logger, string physicalPath, Exception excepcion);

    [LoggerMessage(EventId = 2004, Level = LogLevel.Error, Message = "Error al registrar metadatos en la base de datos para {PhysicalPath}. Procediendo a eliminar archivo físico huérfano.")]
    public static partial void ErrorRegistroBaseDatos(ILogger logger, string physicalPath, Exception excepcion);

    [LoggerMessage(EventId = 2005, Level = LogLevel.Warning, Message = "No se pudo eliminar el archivo huérfano {PhysicalPath}")]
    public static partial void ErrorEliminarHuerfano(ILogger logger, string physicalPath, Exception excepcion);

    [LoggerMessage(EventId = 2006, Level = LogLevel.Information, Message = "Archivo adjunto subido exitosamente con ID {Id}: {NombreCorto} -> {RutaRelativa}")]
    public static partial void ArchivoSubidoExitosamente(ILogger logger, int id, string nombreCorto, string rutaRelativa);
}
