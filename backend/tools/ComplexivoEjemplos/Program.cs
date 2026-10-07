using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using TitulacionIstpet.Application.Common.Models;
using TitulacionIstpet.Application.Features.Complexivo;
using TitulacionIstpet.Infrastructure.Persistence;
using TitulacionIstpet.Infrastructure.Persistence.Repositories;

namespace TitulacionIstpet.Tools.ComplexivoEjemplos;

internal static class Program
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private static async Task<int> Main(string[] args)
    {
        try
        {
            var ejemplos = JsonSerializer.Deserialize<EjemplosComplexivo>(
                await File.ReadAllTextAsync(Path.Combine(AppContext.BaseDirectory, "complexivo-diseno-grafico.json")),
                JsonOptions)
                ?? throw new InvalidOperationException("El archivo de ejemplos está vacío.");
            var validator = new GuardarMallaComplexivoValidator();
            foreach (var modulo in ejemplos.Modulos)
            {
                // Valida el contenido del ejemplo sin conexión; los FK se resuelven después.
                var dto = CrearDto(modulo, 1, null, _ => 1);
                var resultado = await validator.ValidateAsync(dto);
                if (!resultado.IsValid)
                {
                    throw new ValidacionException(resultado.Errors);
                }
                Console.WriteLine($"{modulo.Nombre}: {modulo.Temas.Count} temas, {modulo.TotalHoras} horas, {modulo.FechaInicio:yyyy-MM-dd} a {modulo.FechaFin:yyyy-MM-dd}.");
            }
            if (args.Length == 0 || args is ["--validar"])
            {
                Console.WriteLine("Ejemplos válidos. Usa --listar para consultar cohortes y --cohorte-carrera ID para cargarlos.");
                return 0;
            }
            var listar = args is ["--listar"];
            int? idContexto = args is ["--cohorte-carrera", var numero] && int.TryParse(numero, out var id) && id > 0 ? id : null;
            if (!listar && !idContexto.HasValue)
            {
                throw new ArgumentException("Uso: --validar | --listar | --cohorte-carrera ID");
            }
            var connection = LeerConexion();
            var options = new DbContextOptionsBuilder<SigafiDbContext>()
                .UseMySql(connection, new MySqlServerVersion(new Version(5, 7, 21))).Options;
            await using var context = new SigafiDbContext(options);
            var repo = new RepositorioComplexivo(context);
            var casos = await repo.ListarCohortesCarrerasAsync(default);
            var candidatos = casos.Where(c => c.EsActivo && Normalizar(c.Carrera) == Normalizar(ejemplos.Carrera)).ToList();
            foreach (var candidato in candidatos)
            {
                Console.WriteLine($"idCohorteCarrera={candidato.IdCohorteCarrera}: {candidato.Cohorte} / {candidato.Periodo} / {candidato.Carrera} / {candidato.ModalidadEstudio}");
            }
            if (listar)
            {
                return 0;
            }
            var caso = candidatos.SingleOrDefault(c => c.IdCohorteCarrera == idContexto)
                ?? throw new InvalidOperationException("Selecciona una de las cohortes/carreras activas de Diseño Gráfico mostradas arriba.");
            var asignaturas = await context.Detallemallas.AsNoTracking()
                .Where(d => d.IdMallaNavigation.IdCarrera == caso.IdCarrera)
                .Select(d => new AsignaturaCatalogo(d.IdAsignatura, d.IdAsignaturaNavigation.Asignatura))
                .Distinct().ToListAsync();
            var profesores = await context.Profesores.AsNoTracking().Where(p => p.Activo == true)
                .Select(p => new ProfesorCatalogo(p.IdProfesor, (p.Nombres ?? string.Empty) + " " + (p.Apellidos ?? string.Empty))).ToListAsync();
            var datos = ejemplos.Modulos.Select(modulo =>
            {
                var coincidencias = modulo.Docente is null ? [] : profesores.Where(p =>
                    modulo.Docente.Split(' ', StringSplitOptions.RemoveEmptyEntries).All(nombre =>
                        Normalizar(p.Nombre).Contains(Normalizar(nombre), StringComparison.Ordinal))).ToList();
                var docente = coincidencias.Count == 1 ? coincidencias[0] : null;
                Console.WriteLine($"{modulo.Nombre}: {(docente is null ? "Docente pendiente" : docente.Nombre)}.");
                return CrearDto(modulo, caso.IdCohorteCarrera, docente?.Id,
                    tema => ResolverAsignatura(tema, asignaturas));
            }).ToList();
            var servicio = new AdministrarComplexivo(repo, validator);
            await using var transaction = await context.Database.BeginTransactionAsync();
            var existentes = await repo.ListarMallaAsync(caso.IdCohorteCarrera, default);
            var creados = new List<int>();
            foreach (var dto in datos)
            {
                if (existentes.Any(m => m.Detalle == dto.Detalle))
                {
                    Console.WriteLine($"Ya existe: {dto.Detalle}. Se conserva el registro actual.");
                    continue;
                }
                creados.Add(await servicio.GuardarAsync(null, dto, default));
            }
            context.ChangeTracker.Clear();
            foreach (var creado in creados)
            {
                var leido = await repo.ObtenerAsync(creado, default)
                    ?? throw new InvalidOperationException("No se pudo verificar un módulo creado.");
                if (leido.TotalHoras != 32 || leido.Temas.Sum(t => t.HorasPorTema) != 32 || leido.Docentes.Single().Horarios.Count != 3)
                {
                    throw new InvalidOperationException("La planificación guardada no coincide con el ejemplo.");
                }
            }
            await transaction.CommitAsync();
            Console.WriteLine($"Carga completada: {creados.Count} módulos nuevos. Carrera: {caso.Carrera}; cohorte: {caso.Cohorte}; período: {caso.Periodo}.");
            return 0;
        }
        catch (ValidacionException error)
        {
            foreach (var campo in error.Errores)
            {
                Console.Error.WriteLine($"{campo.Key}: {string.Join(' ', campo.Value)}");
            }
            return 1;
        }
        catch (Exception error)
        {
            Console.Error.WriteLine(error.Message);
            return 1;
        }
    }

    private static GuardarMallaComplexivoDto CrearDto(ModuloEjemplo modulo, int idContexto, string? idProfesor, Func<TemaEjemplo, int> asignatura)
        => new(idContexto, modulo.Nombre, modulo.Descripcion, modulo.FechaInicio, modulo.FechaFin,
            modulo.Modalidad, modulo.TotalHoras, true,
            modulo.Temas.Select(t => new TemaComplexivoDto(0, asignatura(t), t.Detalle, t.Horas)).ToList(),
            [new DocenteComplexivoDto(0, idProfesor, idProfesor is null ? null : new DateOnly(2026, 10, 5),
                $"DEMO-M{modulo.FechaInicio.Month}{modulo.FechaInicio.Day}", true,
                modulo.Horarios.Select(h => new HorarioComplexivoDto(0, h.Dias, h.FranjaHoraria)).ToList())]);

    private static int ResolverAsignatura(TemaEjemplo tema, List<AsignaturaCatalogo> catalogo)
    {
        foreach (var alias in tema.Asignaturas)
        {
            var candidatas = catalogo.Where(a => Normalizar(a.Nombre) == Normalizar(alias)).ToList();
            if (candidatas.Count == 1)
            {
                return candidatas[0].Id;
            }
            if (candidatas.Count > 1)
            {
                throw new InvalidOperationException($"Hay varias asignaturas llamadas {alias}. Revisa la malla antes de cargar el ejemplo.");
            }
        }
        throw new InvalidOperationException($"No se encontró {tema.Asignaturas[0]} en la malla. Ajusta sus nombres en docs/ejemplos/complexivo-diseno-grafico.json. Disponibles: {string.Join(", ", catalogo.Select(a => a.Nombre))}.");
    }

    private static string Normalizar(string texto)
        => string.Concat(texto.Normalize(NormalizationForm.FormD).Where(char.IsLetterOrDigit)).ToUpperInvariant();

    private static string LeerConexion()
    {
        var variable = Environment.GetEnvironmentVariable("ConnectionStrings__SigafiDb");
        if (!string.IsNullOrWhiteSpace(variable))
        {
            return variable;
        }
        var directorio = new DirectoryInfo(Directory.GetCurrentDirectory());
        while (directorio != null && !Directory.Exists(Path.Combine(directorio.FullName, "backend", "src")))
        {
            directorio = directorio.Parent;
        }
        if (directorio == null)
        {
            throw new InvalidOperationException("Ejecuta la herramienta desde el repositorio o define ConnectionStrings__SigafiDb.");
        }
        foreach (var archivo in new[] { "appsettings.Development.json", "appsettings.json" })
        {
            var ruta = Path.Combine(directorio.FullName, "backend", "src", "TitulacionIstpet.WebApi", archivo);
            if (!File.Exists(ruta))
            {
                continue;
            }
            using var config = JsonDocument.Parse(File.ReadAllText(ruta));
            if (config.RootElement.TryGetProperty("ConnectionStrings", out var cadenas) &&
                cadenas.TryGetProperty("SigafiDb", out var valor) && !string.IsNullOrWhiteSpace(valor.GetString()))
            {
                return valor.GetString()!;
            }
        }
        throw new InvalidOperationException("No se encontró ConnectionStrings:SigafiDb en la configuración local.");
    }
}

internal sealed record EjemplosComplexivo(string Carrera, List<ModuloEjemplo> Modulos);
internal sealed record ModuloEjemplo(string Nombre, string Descripcion, DateOnly FechaInicio, DateOnly FechaFin,
    int TotalHoras, string Modalidad, string? Docente, List<TemaEjemplo> Temas, List<HorarioEjemplo> Horarios);
internal sealed record TemaEjemplo(string[] Asignaturas, string Detalle, int Horas);
internal sealed record HorarioEjemplo(string Dias, string FranjaHoraria);
internal sealed record AsignaturaCatalogo(int Id, string Nombre);
internal sealed record ProfesorCatalogo(string Id, string Nombre);
