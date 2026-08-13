namespace Titan.Application.DTOs.Academico;

public sealed record ModalidadDto(
    int IdModalidad,
    string Modalidad,
    string? ModalidadImpresion
);

public sealed record ModalidadCarreraDto(
    int IdModalidadCarrera,
    int IdCarrera,
    string Carrera,
    int IdModalidad,
    string Modalidad,
    bool EsActivo
);

public sealed record SistemaTitulacionDto(
    int CodigoSistema,
    string Detalle,
    bool Activo
);

public sealed record EstudianteModalidadContextDto(
    int IdCarrera,
    string NombreCarrera,
    int IdModalidadEstudio,
    string NombreModalidadEstudio,
    IEnumerable<ModalidadDto> ModalidadesDisponiblesCarrera,
    IEnumerable<SistemaTitulacionDto> OpcionesTitulacionDisponibles
);
