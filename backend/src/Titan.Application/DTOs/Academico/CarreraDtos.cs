namespace Titan.Application.DTOs.Academico;

public sealed record CarreraDto(
    int IdCarrera,
    string NombreCarrera,
    string? AliasCarrera,
    string? CodigoCases,
    bool Activa,
    int? IdModalidad = null,
    string? NombreModalidad = null
);

public sealed record EstudianteCarreraDto(
    int IdCarrera,
    string NombreCarrera,
    string? AliasCarrera,
    bool EstaTitulado,
    string? CodigoSistemaTitulacion,
    bool TieneMatriculaVigente,
    int? IdModalidad = null,
    string? NombreModalidad = null
);

public sealed record ProfesorCarreraDto(
    int IdCarrera,
    string NombreCarrera,
    string? AliasCarrera,
    bool AsignadoEnTodasLasCarreras,
    string? PeriodoAcademico,
    int? IdModalidad = null,
    string? NombreModalidad = null
);

public sealed record UsuarioCarrerasResponseDto(
    string IdSigafi,
    string NombreUsuario,
    string TipoUsuario,
    IEnumerable<EstudianteCarreraDto> CarrerasEstudiante,
    IEnumerable<ProfesorCarreraDto> CarrerasDocente
);
