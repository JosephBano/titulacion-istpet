namespace TitulacionIstpet.Application.Features.ConfiguracionGeneral.DTOs;

public sealed record ModalidadMaestraDto(
    int IdModalidadTitulacion,
    string ModalidadTitulacion,
    string? EsComplexivo,
    string? EsArticuloCientifico,
    string? GeneraTesis,
    int? CantidadMinima,
    bool EsActivo,
    int TotalRequisitosAsociados
);

public sealed record CrearModalidadMaestraDto(
    string ModalidadTitulacion,
    string? EsComplexivo = "NO",
    string? EsArticuloCientifico = "NO",
    string? GeneraTesis = "NO",
    int? CantidadMinima = 1
);

public sealed record ActualizarModalidadMaestraDto(
    int IdModalidadTitulacion,
    string ModalidadTitulacion,
    string? EsComplexivo,
    string? EsArticuloCientifico,
    string? GeneraTesis,
    int? CantidadMinima,
    bool EsActivo
);

public sealed record RequisitoMaestroDto(
    int IdRequisitos,
    string Requisito,
    bool EsAdjunto,
    bool EsBool,
    bool SubeAlumno,
    bool SubeColaborador,
    bool EsActivo
);

public sealed record CrearRequisitoMaestroDto(
    string Requisito,
    bool EsAdjunto = true,
    bool EsBool = false,
    bool SubeAlumno = true,
    bool SubeColaborador = false
);

public sealed record ActualizarRequisitoMaestroDto(
    int IdRequisitos,
    string Requisito,
    bool EsAdjunto,
    bool EsBool,
    bool SubeAlumno,
    bool SubeColaborador,
    bool EsActivo
);

public sealed record RequisitoModalidadMatrizDto(
    int IdRequisitoModalidad,
    int IdModalidadTitulacion,
    string ModalidadTitulacion,
    int IdRequisitos,
    string NombreRequisito,
    bool EsAdjunto,
    bool EsBool,
    bool SubeAlumno,
    bool SubeColaborador,
    bool EsRequisitoFinal,
    bool EsActivo
);

public sealed record AsignarRequisitoModalidadDto(
    int IdModalidadTitulacion,
    int IdRequisitos,
    bool EsRequisitoFinal = false
);

public sealed record ResumenGeneralSistemaDto(
    string? PeriodoCodigo,
    string? PeriodoNombreHumano,
    string? ConvocatoriaDetalle,
    DateTime? FechaInicioCorte,
    DateTime? FechaFinCorte,
    int? DiasRestantesCorte,
    bool EstaVigenteCorte,
    int TotalCarrerasHabilitadas,
    int TotalModalidadesActivas,
    int TotalRequisitosActivos,
    int TotalPostulaciones,
    int TotalAprobadas,
    int TotalEnRevision,
    int TotalObservadas,
    int TotalRechazadas,
    string EstadoOperativo
);
