namespace TitulacionIstpet.Application.Features.Postulaciones.DTOs;

public sealed record ElegibilidadPostulacionDto(
    bool EsElegible,
    string? Mensaje,
    int? IdMatricula,
    string? IdAlumno,
    string? NombreCompleto,
    int? IdCarrera,
    string? NombreCarrera,
    int? IdCohorte,
    string? DetalleCohorte,
    bool TienePostulacionActiva,
    int? IdPostulacionActiva,
    string? EstadoPostulacionActiva,
    IReadOnlyList<ModalidadOfertadaDto> ModalidadesOfertadas
);

public sealed record ModalidadOfertadaDto(
    int IdModalidadTitulacionCarrera,
    int IdModalidadTitulacion,
    string ModalidadTitulacion,
    string? EsComplexivo,
    string? EsArticuloCientifico,
    string? GeneraTesis,
    IReadOnlyList<RequisitoModalidadOfertadaDto> Requisitos
);

public sealed record RequisitoModalidadOfertadaDto(
    int IdRequisitoModalidad,
    int IdRequisitos,
    string NombreRequisito,
    bool EsAdjunto,
    bool EsBool,
    bool SubeAlumno,
    bool SubeColaborador,
    bool EsRequisitoFinal
);

public sealed record PostulacionResumenDto(
    int IdPostulacionAlumnos,
    int IdMatricula,
    string IdAlumno,
    string NombreAlumno,
    string CedulaAlumno,
    int IdCarrera,
    string NombreCarrera,
    int IdCohorte,
    string DetalleCohorte,
    int IdModalidadTitulacionCarrera,
    string ModalidadTitulacion,
    int IdPostulacionEstado,
    string NombreEstado,
    bool EsActivo,
    bool? EsCambioModalidad,
    int TotalRequisitos,
    int TotalRequisitosCompletados
);

public sealed record PostulacionDetalleDto(
    int IdPostulacionAlumnos,
    int IdMatricula,
    string IdAlumno,
    string NombreAlumno,
    string CedulaAlumno,
    string EmailAlumno,
    string TelefonoAlumno,
    int IdCarrera,
    string NombreCarrera,
    int IdCohorte,
    string DetalleCohorte,
    int IdModalidadTitulacionCarrera,
    string ModalidadTitulacion,
    int IdPostulacionEstado,
    string NombreEstado,
    bool EsActivo,
    bool? EsCambioModalidad,
    IReadOnlyList<PostulacionRequisitoDetalleDto> Requisitos,
    string? ObservacionDictamen = null
);

public sealed record PostulacionRequisitoDetalleDto(
    int IdPostulacionAlumnoRequisitoModalidad,
    int IdPostulacionAlumnos,
    int IdRequisitoModalidad,
    int IdRequisitos,
    string NombreRequisito,
    bool EsAdjunto,
    bool EsBool,
    bool SubeAlumno,
    int? IdAdjuntosImagenes,
    string? NombreArchivoAdjunto,
    string? RutaArchivoAdjunto,
    bool? ValorBool,
    string? EstadoValidacion = "PENDIENTE",
    string? Observaciones = null,
    string? NombreEvaluador = null,
    string? CedulaEvaluador = null,
    DateTime? FechaEvaluacion = null
);

public sealed record EstadoPostulacionDto(
    int IdPostulacionEstado,
    string Nombre,
    int Orden,
    bool EsFinal,
    bool EsActivo
);

public sealed record PaginaPostulacionesDto(
    IReadOnlyList<PostulacionResumenDto> Items,
    int Pagina,
    int TamanoPagina,
    int Total
);

public sealed record TotalPostulacionesDto(
    int TotalPostulaciones
);

public sealed record RequisitoPostulacionInputDto(
    int IdRequisitoModalidad,
    int? IdAdjuntosImagenes,
    bool? ValorBool
);

public sealed record PortalEstudianteDto(
    ConvocatoriaPortalDto Convocatoria,
    EstudiantePortalDto Estudiante,
    PostulacionDetalleDto? PostulacionActiva,
    IReadOnlyList<ModalidadOfertadaDto> ModalidadesDisponibles
);

public sealed record ConvocatoriaPortalDto(
    bool EstaAbierta,
    string? Periodo,
    string? Detalle,
    DateTime? FechaInicio,
    DateTime? FechaCierre,
    int? DiasRestantes,
    string Mensaje
);

public sealed record EstudiantePortalDto(
    string IdAlumno,
    string Cedula,
    string NombreCompleto,
    string? Email,
    string? Celular,
    int? IdCarrera,
    string? NombreCarrera,
    int? IdMatricula,
    bool EsElegible,
    string MensajeElegibilidad
);

public sealed record DictamenPostulacionComando(
    int IdPostulacionAlumnos,
    string Decision,
    string? Observaciones,
    IReadOnlyList<int>? IdsRequisitosObservados = null
);
