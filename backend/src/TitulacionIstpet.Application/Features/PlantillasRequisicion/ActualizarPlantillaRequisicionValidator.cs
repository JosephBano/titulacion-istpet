using System.Text.Json;
using System.Text.RegularExpressions;
using FluentValidation;

namespace TitulacionIstpet.Application.Features.PlantillasRequisicion;

public sealed partial class ActualizarPlantillaRequisicionValidator : AbstractValidator<ActualizarPlantillaRequisicionComandoDto>
{
    private static readonly HashSet<string> FuentesPermitidas = ["sans", "serif", "mono"];
    private static readonly HashSet<string> AjustesFondoPermitidos = ["contain", "cover"];
    private static readonly HashSet<string> FuentesValoresPermitidas = ["literal", "contexto", "texto"];
    private static readonly HashSet<string> ClavesContextoPermitidas =
    [
        "fechaActual",
        "solicitante.nombre",
        "curso.carrera",
        "curso.cohorte",
        "curso.numeroVacantes"
    ];

    private static readonly string[] SeccionesRequeridas =
    [
        "general",
        "institucion",
        "vacante",
        "motivo",
        "perfil",
        "observaciones"
    ];

    private static readonly string[] CamposRequeridos =
    [
        "fechaSolicitud",
        "area",
        "solicitante",
        "cargo",
        "numeroVacantes",
        "institucion",
        "contratacion",
        "jornada",
        "remuneracion",
        "valorRemuneracion",
        "valorTotal",
        "horario",
        "lugar",
        "fechaIngreso",
        "motivo",
        "otroMotivo",
        "detalleMotivo",
        "formacion",
        "experiencia",
        "conocimientos",
        "competencias",
        "otraCompetencia",
        "observaciones"
    ];

    private static readonly string[] FirmasRequeridas = ["requerido", "aprobado", "rrhh"];

    [GeneratedRegex("^#([0-9a-fA-F]{3}|[0-9a-fA-F]{6})$")]
    private static partial Regex HexColorRegex();

    public ActualizarPlantillaRequisicionValidator()
    {
        RuleFor(x => x.RevisionEsperada)
            .GreaterThanOrEqualTo(0)
            .WithMessage("La revisión esperada debe ser mayor o igual a 0.");

        RuleFor(x => x.Definicion)
            .NotNull()
            .WithMessage("La definición de la plantilla es obligatoria.");

        When(x => x.Definicion != null, () =>
        {
            RuleFor(x => x.Definicion.SchemaVersion)
                .Equal(PlantillaRequisicionBase.SchemaVersionActual)
                .WithMessage($"La versión de esquema debe ser {PlantillaRequisicionBase.SchemaVersionActual}.");

            RuleFor(x => x.Definicion.Codigo)
                .Equal(PlantillaRequisicionBase.CodigoRequisicion)
                .WithMessage($"El código debe ser '{PlantillaRequisicionBase.CodigoRequisicion}'.");

            // Tamaño máximo de serialización
            RuleFor(x => x.Definicion)
                .Must(def =>
                {
                    try
                    {
                        var json = JsonSerializer.Serialize(def, PlantillaRequisicionBase.JsonOptions);
                        return System.Text.Encoding.UTF8.GetByteCount(json) <= PlantillaRequisicionBase.MaximoBytesDefinicion;
                    }
                    catch
                    {
                        return false;
                    }
                })
                .WithMessage("La definición serializada no debe exceder los 128 KiB.");

            // Documento
            RuleFor(x => x.Definicion.Documento)
                .NotNull()
                .WithMessage("El bloque documento es obligatorio.");

            When(x => x.Definicion.Documento != null, () =>
            {
                RuleFor(x => x.Definicion.Documento.Titulo)
                    .NotEmpty().WithMessage("El título del documento es obligatorio.")
                    .MaximumLength(200).WithMessage("El título no debe exceder 200 caracteres.");

                RuleFor(x => x.Definicion.Documento.Subtitulo)
                    .MaximumLength(200).WithMessage("El subtítulo no debe exceder 200 caracteres.");

                RuleFor(x => x.Definicion.Documento.NotaFinal)
                    .NotEmpty().WithMessage("La nota final es obligatoria.")
                    .MaximumLength(1000).WithMessage("La nota final no debe exceder 1000 caracteres.");

                // Pagina
                RuleFor(x => x.Definicion.Documento.Pagina)
                    .NotNull().WithMessage("La configuración de página es obligatoria.");

                When(x => x.Definicion.Documento.Pagina != null, () =>
                {
                    RuleFor(x => x.Definicion.Documento.Pagina.Formato)
                        .Equal("A4").WithMessage("El formato de página debe ser 'A4'.");

                    RuleFor(x => x.Definicion.Documento.Pagina.Orientacion)
                        .Equal("vertical").WithMessage("La orientación de página debe ser 'vertical'.");

                    RuleFor(x => x.Definicion.Documento.Pagina.MargenesMm)
                        .NotNull().WithMessage("Los márgenes son obligatorios.");

                    When(x => x.Definicion.Documento.Pagina.MargenesMm != null, () =>
                    {
                        RuleFor(x => x.Definicion.Documento.Pagina.MargenesMm.Superior)
                            .InclusiveBetween(8, 25).WithMessage("El margen superior debe estar entre 8 y 25 mm.");
                        RuleFor(x => x.Definicion.Documento.Pagina.MargenesMm.Derecho)
                            .InclusiveBetween(8, 25).WithMessage("El margen derecho debe estar entre 8 y 25 mm.");
                        RuleFor(x => x.Definicion.Documento.Pagina.MargenesMm.Inferior)
                            .InclusiveBetween(8, 25).WithMessage("El margen inferior debe estar entre 8 y 25 mm.");
                        RuleFor(x => x.Definicion.Documento.Pagina.MargenesMm.Izquierdo)
                            .InclusiveBetween(8, 25).WithMessage("El margen izquierdo debe estar entre 8 y 25 mm.");
                    });
                });

                // Estilo
                RuleFor(x => x.Definicion.Documento.Estilo)
                    .NotNull().WithMessage("La configuración de estilo es obligatoria.");

                When(x => x.Definicion.Documento.Estilo != null, () =>
                {
                    RuleFor(x => x.Definicion.Documento.Estilo.Fuente)
                        .Must(f => FuentesPermitidas.Contains(f))
                        .WithMessage("La fuente debe ser 'sans', 'serif' o 'mono'.");

                    RuleFor(x => x.Definicion.Documento.Estilo.TamanoFuentePt)
                        .InclusiveBetween(8, 12).WithMessage("El tamaño de fuente debe estar entre 8 y 12 pt.");

                    RuleFor(x => x.Definicion.Documento.Estilo.ColorTexto)
                        .Must(EsHexColor).WithMessage("El color de texto debe ser un código hexadecimal válido (#RGB o #RRGGBB).");

                    RuleFor(x => x.Definicion.Documento.Estilo.ColorEncabezado)
                        .Must(EsHexColor).WithMessage("El color de encabezado debe ser un código hexadecimal válido.");

                    RuleFor(x => x.Definicion.Documento.Estilo.ColorDestacado)
                        .Must(EsHexColor).WithMessage("El color destacado debe ser un código hexadecimal válido.");

                    RuleFor(x => x.Definicion.Documento.Estilo.ColorFondo)
                        .Must(EsHexColor).WithMessage("El color de fondo debe ser un código hexadecimal válido.");

                    RuleFor(x => x.Definicion.Documento.Estilo.ColorBorde)
                        .Must(EsHexColor).WithMessage("El color de borde debe ser un código hexadecimal válido.");

                    RuleFor(x => x.Definicion.Documento.Estilo.GrosorBordeMm)
                        .InclusiveBetween(0.2, 1.0).WithMessage("El grosor de borde debe estar entre 0.2 y 1.0 mm.");
                });

                // Logo
                RuleFor(x => x.Definicion.Documento.Logo)
                    .NotNull().WithMessage("La configuración de logo es obligatoria.");

                When(x => x.Definicion.Documento.Logo != null, () =>
                {
                    RuleFor(x => x.Definicion.Documento.Logo.AnchoMm)
                        .InclusiveBetween(10, 80).WithMessage("El ancho del logo debe estar entre 10 y 80 mm.");

                    When(x => x.Definicion.Documento.Logo.IdAdjuntosImagenes.HasValue, () =>
                    {
                        RuleFor(x => x.Definicion.Documento.Logo.IdAdjuntosImagenes!.Value)
                            .GreaterThan(0).WithMessage("El id de imagen del logo debe ser positivo.");
                    });
                });

                // Fondo
                RuleFor(x => x.Definicion.Documento.Fondo)
                    .NotNull().WithMessage("La configuración de fondo es obligatoria.");

                When(x => x.Definicion.Documento.Fondo != null, () =>
                {
                    RuleFor(x => x.Definicion.Documento.Fondo.Opacidad)
                        .InclusiveBetween(0.0, 1.0).WithMessage("La opacidad del fondo debe estar entre 0 y 1.");

                    RuleFor(x => x.Definicion.Documento.Fondo.Ajuste)
                        .Must(a => AjustesFondoPermitidos.Contains(a))
                        .WithMessage("El ajuste de fondo debe ser 'contain' o 'cover'.");

                    When(x => x.Definicion.Documento.Fondo.IdAdjuntosImagenes.HasValue, () =>
                    {
                        RuleFor(x => x.Definicion.Documento.Fondo.IdAdjuntosImagenes!.Value)
                            .GreaterThan(0).WithMessage("El id de imagen de fondo debe ser positivo.");
                    });
                });
            });

            // Secciones
            RuleFor(x => x.Definicion.Secciones)
                .NotNull().WithMessage("Las secciones son obligatorias.")
                .Must(secs => secs != null && SeccionesRequeridas.All(id => secs.Any(s => s.Id == id)))
                .WithMessage("Deben estar presentes las 6 secciones requeridas (general, institucion, vacante, motivo, perfil, observaciones).");

            RuleForEach(x => x.Definicion.Secciones).ChildRules(sec =>
            {
                sec.RuleFor(s => s.Id).NotEmpty().MaximumLength(50);
                sec.RuleFor(s => s.Titulo).NotEmpty().MaximumLength(200);
            });

            // Campos
            RuleFor(x => x.Definicion.Campos)
                .NotNull().WithMessage("Los campos son obligatorios.")
                .Must(campos => campos != null && CamposRequeridos.All(c => campos.ContainsKey(c)))
                .WithMessage("Deben estar presentes los 23 campos requeridos de la requisición.");

            When(x => x.Definicion.Campos != null, () =>
            {
                RuleForEach(x => x.Definicion.Campos).ChildRules(kvp =>
                {
                    kvp.RuleFor(c => c.Value.Etiqueta)
                        .NotEmpty().WithMessage("La etiqueta del campo es obligatoria.")
                        .MaximumLength(200).WithMessage("La etiqueta no debe exceder 200 caracteres.");

                    kvp.RuleFor(c => c.Value.Ayuda)
                        .MaximumLength(500).WithMessage("El texto de ayuda no debe exceder 500 caracteres.");
                });
            });

            // Opciones
            RuleFor(x => x.Definicion.Opciones)
                .NotNull().WithMessage("El catálogo de opciones es obligatorio.");

            When(x => x.Definicion.Opciones != null, () =>
            {
                ValidarCatalogo(RuleFor(x => x.Definicion.Opciones.Institucion), "institucion");
                ValidarCatalogo(RuleFor(x => x.Definicion.Opciones.Contratacion), "contratacion");
                ValidarCatalogo(RuleFor(x => x.Definicion.Opciones.Jornada), "jornada");
                ValidarCatalogo(RuleFor(x => x.Definicion.Opciones.Remuneracion), "remuneracion");
                ValidarCatalogo(RuleFor(x => x.Definicion.Opciones.Motivo), "motivo");
                ValidarCatalogo(RuleFor(x => x.Definicion.Opciones.Competencias), "competencias");
            });

            // Valores Iniciales
            RuleFor(x => x.Definicion.ValoresIniciales)
                .NotNull().WithMessage("El mapa de valores iniciales es obligatorio.");

            When(x => x.Definicion.ValoresIniciales != null, () =>
            {
                RuleForEach(x => x.Definicion.ValoresIniciales).ChildRules(kvp =>
                {
                    kvp.RuleFor(v => v.Value.Fuente)
                        .Must(f => FuentesValoresPermitidas.Contains(f))
                        .WithMessage("La fuente de valor inicial debe ser 'literal', 'contexto' o 'texto'.");

                    kvp.When(v => v.Value.Fuente == "contexto", () =>
                    {
                        kvp.RuleFor(v => v.Value.Clave)
                            .NotEmpty().WithMessage("La clave de contexto es obligatoria cuando la fuente es 'contexto'.")
                            .Must(c => c != null && ClavesContextoPermitidas.Contains(c))
                            .WithMessage("La clave de contexto debe ser una variable admitida (fechaActual, solicitante.nombre, curso.carrera, curso.cohorte, curso.numeroVacantes).");
                    });

                    kvp.When(v => v.Value.Fuente == "texto", () =>
                    {
                        kvp.RuleFor(v => v.Value.Valor)
                            .NotEmpty().WithMessage("El valor de texto es obligatorio cuando la fuente es 'texto'.")
                            .Must(ValidarVariablesEnTexto)
                            .WithMessage("El texto solo puede contener variables de contexto admitidas.");
                    });
                });
            });

            // Firmas
            RuleFor(x => x.Definicion.Firmas)
                .NotNull().WithMessage("Las firmas son obligatorias.")
                .Must(firmas => firmas != null && FirmasRequeridas.All(id => firmas.Any(f => f.Id == id)))
                .WithMessage("Deben estar presentes las 3 firmas requeridas (requerido, aprobado, rrhh).");

            RuleForEach(x => x.Definicion.Firmas).ChildRules(firma =>
            {
                firma.RuleFor(f => f.Titulo).NotEmpty().MaximumLength(100);
                firma.RuleFor(f => f.EtiquetaFirma).NotEmpty().MaximumLength(100);
                firma.RuleFor(f => f.EtiquetaNombre).NotEmpty().MaximumLength(100);
                firma.RuleFor(f => f.EtiquetaCargo).NotEmpty().MaximumLength(100);
            });
        });
    }

    private static void ValidarCatalogo(IRuleBuilder<ActualizarPlantillaRequisicionComandoDto, List<OpcionItemDto>> builder, string catalogo)
    {
        builder
            .NotEmpty().WithMessage($"El catálogo de {catalogo} no debe estar vacío.")
            .Must(items => items.Select(i => i.Id).Distinct().Count() == items.Count)
            .WithMessage($"Los identificadores del catálogo {catalogo} deben ser únicos.");

        builder.ForEach(item =>
        {
            item.Must(i => !string.IsNullOrWhiteSpace(i.Id) && i.Id.Length <= 50)
                .WithMessage($"El id de opción en {catalogo} es obligatorio y máximo de 50 caracteres.");
            item.Must(i => !string.IsNullOrWhiteSpace(i.Etiqueta) && i.Etiqueta.Length <= 200)
                .WithMessage($"La etiqueta de opción en {catalogo} es obligatoria y máximo de 200 caracteres.");
        });
    }

    private static bool EsHexColor(string? color)
    {
        return !string.IsNullOrWhiteSpace(color) && HexColorRegex().IsMatch(color);
    }

    private static bool ValidarVariablesEnTexto(string? texto)
    {
        if (string.IsNullOrWhiteSpace(texto))
        {
            return true;
        }
        var matches = Regex.Matches(texto, @"\{\{([^}]+)\}\}");
        foreach (Match match in matches)
        {
            var variable = match.Groups[1].Value.Trim();
            if (!ClavesContextoPermitidas.Contains(variable))
            {
                return false;
            }
        }
        return true;
    }
}
