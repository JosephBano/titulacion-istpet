using System.Text.Json;

namespace TitulacionIstpet.Application.Features.PlantillasRequisicion;

public static class PlantillaRequisicionBase
{
    public const string CodigoRequisicion = "requisicion-personal";
    public const int SchemaVersionActual = 1;
    public const int MaximoBytesDefinicion = 128 * 1024; // 128 KiB

    public static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = false
    };

    public static PlantillaRequisicionDefinicionDto CrearDefinicionPredeterminada()
    {
        return new PlantillaRequisicionDefinicionDto(
            SchemaVersion: SchemaVersionActual,
            Codigo: CodigoRequisicion,
            Documento: new DocumentoPlantillaDto(
                Titulo: "FORMATO DE REQUISICIÓN DE PERSONAL",
                Subtitulo: "",
                Pagina: new PaginaPlantillaDto(
                    Formato: "A4",
                    Orientacion: "vertical",
                    MargenesMm: new MargenesMmDto(
                        Superior: 14,
                        Derecho: 14,
                        Inferior: 14,
                        Izquierdo: 14
                    )
                ),
                Estilo: new EstiloPlantillaDto(
                    Fuente: "sans",
                    TamanoFuentePt: 9,
                    ColorTexto: "#171717",
                    ColorEncabezado: "#171717",
                    ColorDestacado: "#002855",
                    ColorFondo: "#FFFFFF",
                    ColorBorde: "#334155",
                    GrosorBordeMm: 0.35
                ),
                Logo: new LogoPlantillaDto(
                    UsarLogoInstitucional: false,
                    IdAdjuntosImagenes: null,
                    AnchoMm: 24
                ),
                Fondo: new FondoPlantillaDto(
                    IdAdjuntosImagenes: null,
                    Opacidad: 0.1,
                    Ajuste: "contain"
                ),
                NotaFinal: "Toda requisición de personal deberá contar con la aprobación de Rectorado y disponibilidad presupuestaria previa al inicio del proceso de reclutamiento y selección."
            ),
            Secciones:
            [
                new SeccionPlantillaDto("general", "1. INFORMACIÓN GENERAL"),
                new SeccionPlantillaDto("institucion", "2. INSTITUCIÓN SOLICITANTE"),
                new SeccionPlantillaDto("vacante", "3. INFORMACIÓN DE LA VACANTE"),
                new SeccionPlantillaDto("motivo", "4. MOTIVO DE LA VACANTE"),
                new SeccionPlantillaDto("perfil", "5. PERFIL REQUERIDO"),
                new SeccionPlantillaDto("observaciones", "6. OBSERVACIONES")
            ],
            Campos: new Dictionary<string, CampoPlantillaDto>(StringComparer.Ordinal)
            {
                ["fechaSolicitud"] = new("Fecha de solicitud", ""),
                ["area"] = new("Área / Departamento", ""),
                ["solicitante"] = new("Nombre del solicitante", ""),
                ["cargo"] = new("Cargo solicitado", ""),
                ["numeroVacantes"] = new("Número de vacantes", ""),
                ["institucion"] = new("Institución solicitante", ""),
                ["contratacion"] = new("Tipo de contratación", ""),
                ["jornada"] = new("Jornada laboral", ""),
                ["remuneracion"] = new("Remuneración", ""),
                ["valorRemuneracion"] = new("Sueldo / valor por hora", ""),
                ["valorTotal"] = new("Valor total", ""),
                ["horario"] = new("Horario requerido", ""),
                ["lugar"] = new("Lugar de trabajo", ""),
                ["fechaIngreso"] = new("Fecha requerida de ingreso", ""),
                ["motivo"] = new("Motivo de la vacante", ""),
                ["otroMotivo"] = new("Otro motivo", ""),
                ["detalleMotivo"] = new("Detalle del motivo", ""),
                ["formacion"] = new("Formación académica", ""),
                ["experiencia"] = new("Experiencia requerida", ""),
                ["conocimientos"] = new("Conocimientos específicos", ""),
                ["competencias"] = new("Competencias requeridas", ""),
                ["otraCompetencia"] = new("Otra competencia", ""),
                ["observaciones"] = new("Observaciones", "")
            },
            Opciones: new OpcionesPlantillaDto(
                Institucion:
                [
                    new("escuela_conduccion_istpet", "Escuela de Conducción del Instituto Superior Tecnológico Mayor Pedro Traversari"),
                    new("istpet", "Instituto Superior Tecnológico Mayor Pedro Traversari"),
                    new("academia_miguel_iturralde", "Unidad Educativa Particular Bilingüe Academia Militar General Miguel Iturralde"),
                    new("academia_miguel_iturralde_2", "Unidad Educativa Bilingüe Particular Academia Militar General Miguel Iturralde 2"),
                    new("ecmi", "Escuela de Conducción No Profesionales ECMI"),
                    new("club_miguel_iturralde", "Club Deportivo Miguel Iturralde")
                ],
                Contratacion:
                [
                    new("contrato", "Contrato"),
                    new("nombramiento", "Nombramiento"),
                    new("servicios_profesionales", "Servicios Profesionales")
                ],
                Jornada:
                [
                    new("tiempo_completo", "Tiempo completo"),
                    new("medio_tiempo", "Medio tiempo"),
                    new("horas", "Horas")
                ],
                Remuneracion:
                [
                    new("sueldo", "Sueldo"),
                    new("factura", "Factura")
                ],
                Motivo:
                [
                    new("reemplazo", "Reemplazo"),
                    new("renuncia", "Renuncia"),
                    new("desvinculacion", "Desvinculación"),
                    new("proyecto_temporal", "Proyecto temporal"),
                    new("incremento_personal", "Incremento de personal"),
                    new("licencia_maternidad", "Licencia / Maternidad"),
                    new("nueva_creacion", "Nueva creación de cargo"),
                    new("otro", "Otro")
                ],
                Competencias:
                [
                    new("liderazgo", "Liderazgo"),
                    new("resultados", "Orientación a resultados"),
                    new("conflictos", "Resolución de conflicto"),
                    new("equipo", "Trabajo en equipo"),
                    new("comunicacion", "Comunicación efectiva"),
                    new("adaptabilidad", "Adaptabilidad"),
                    new("organizacion", "Organización"),
                    new("otro", "Otro")
                ]
            ),
            ValoresIniciales: new Dictionary<string, ValorInicialPlantillaDto>(StringComparer.Ordinal)
            {
                ["fechaSolicitud"] = new("contexto", null, "fechaActual"),
                ["solicitante"] = new("contexto", null, "solicitante.nombre"),
                ["area"] = new("literal", "Unidad de Titulación", null),
                ["institucion"] = new("literal", "istpet", null),
                ["cargo"] = new("texto", "Docente para curso complexivo de la carrera de {{curso.carrera}}", null),
                ["numeroVacantes"] = new("contexto", null, "curso.numeroVacantes"),
                ["contratacion"] = new("literal", "servicios_profesionales", null),
                ["jornada"] = new("literal", "horas", null),
                ["remuneracion"] = new("literal", "factura", null),
                ["motivo"] = new("literal", "proyecto_temporal", null)
            },
            Firmas:
            [
                new("requerido", "REQUERIDO POR:", "Firma", "Nombre", "Cargo"),
                new("aprobado", "APROBADO POR:", "Firma", "Nombre", "Cargo"),
                new("rrhh", "RECIBIDO POR: RRHH", "Firma", "Nombre", "Cargo")
            ]
        );
    }
}
