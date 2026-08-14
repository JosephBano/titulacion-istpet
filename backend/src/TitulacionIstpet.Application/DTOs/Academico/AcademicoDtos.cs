namespace TitulacionIstpet.Application.DTOs.Academico;

public sealed record CarreraResponseDto(
    int IdCarrera,
    string Nombre,
    string Alias,
    string CodigoCases,
    string DirectorCarrera,
    bool Activa
);

public sealed record PeriodoResponseDto(
    string IdPeriodo,
    string Nombre,
    DateOnly? FechaInicio,
    DateOnly? FechaFin,
    bool Activo
);

public sealed record AsignaturaResponseDto(
    int IdAsignatura,
    string Nombre,
    int? Creditos,
    int? Horas,
    int? Nivel
);

public sealed record ModalidadResponseDto(
    int IdModalidad,
    string Nombre
);
