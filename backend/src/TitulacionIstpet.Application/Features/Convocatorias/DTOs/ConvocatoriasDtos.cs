namespace TitulacionIstpet.Application.Features.Convocatorias.DTOs;

public sealed record AperturarPeriodoConvocatoriaComando(
    string IdPeriodo,
    string DetalleConvocatoria,
    DateTime FechaInicioCorte,
    DateTime FechaFinCorte,
    int DiasPermitidos = 90,
    int DiasExtension = 30,
    bool HabilitarTodasLasCarreras = true,
    IReadOnlyList<int>? IdsModalidadesCarrerasHabilitadas = null,
    IReadOnlyList<int>? IdsCarrerasHabilitadas = null,
    IReadOnlyList<int>? IdsModalidadesHabilitadas = null
);

public sealed record AjustarFechasCorteComando(
    int IdCohorte,
    DateTime? FechaInicio,
    DateTime? FechaFin,
    int? DiasPermitidos,
    int? DiasExtension,
    bool? EsActivo
);

public sealed record ConvocatoriaResumenDto(
    int IdCohorte,
    string IdPeriodo,
    string Detalle,
    DateTime? FechaInicio,
    DateTime? FechaFin,
    int? DiasPermitidos,
    int? DiasExtension,
    bool EsActivo,
    bool EstaVigenteCorte,
    int TotalCarrerasHabilitadas,
    int TotalPostulaciones
);

public sealed record ConvocatoriaDetalleDto(
    int IdCohorte,
    string IdPeriodo,
    string Detalle,
    DateTime? FechaInicio,
    DateTime? FechaFin,
    int? DiasPermitidos,
    int? DiasExtension,
    bool EsActivo,
    bool EstaVigenteCorte,
    IReadOnlyList<CarreraConvocatoriaDto> CarrerasHabilitadas
);

public sealed record CarreraConvocatoriaDto(
    int IdCohorteCarrera,
    int IdModalidadCarrera,
    int IdCarrera,
    string NombreCarrera,
    int IdModalidadEstudio,
    string NombreModalidadEstudio,
    bool EsActivo,
    IReadOnlyList<ModalidadTitulacionHabilitadaDto> ModalidadesTitulacion
);

public sealed record ModalidadTitulacionHabilitadaDto(
    int IdModalidadTitulacionCarrera,
    int IdModalidadTitulacion,
    string NombreModalidadTitulacion,
    bool EsActivo,
    int TotalRequisitosConfigurados
);

public sealed record ConmutarModalidadCarreraComando(
    int IdModalidadTitulacionCarrera,
    bool EsActivo
);
