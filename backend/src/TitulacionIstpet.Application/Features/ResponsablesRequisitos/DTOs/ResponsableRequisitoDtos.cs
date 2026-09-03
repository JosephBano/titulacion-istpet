namespace TitulacionIstpet.Application.Features.ResponsablesRequisitos.DTOs;

public sealed record ResponsableRequisitoDto(
    int IdResponsableEvidencias,
    int IdRequisitos,
    string NombreRequisito,
    string IdProfesor,
    string NombreProfesor,
    string EmailProfesor,
    bool Activo
);

public sealed record AsignarProfesorRequisitoDto(
    int IdRequisitos,
    string IdProfesor
);

public sealed record ProfesorCandidatoDto(
    string IdProfesor,
    string NombresCompletos,
    string Email,
    string Celular,
    bool Activo
);

public sealed record EvaluacionDocenteItemDto(
    int IdTitulResponsableEvidencia,
    int IdPostulacionAlumnoRequisitoModalidad,
    int IdResponsableEvidencias,
    string Estado,
    string? Observaciones,
    DateTime? Actualizado,
    string? EvaluadorNombre
);

public sealed record RequisitoEvaluacionDocenteDto(
    int IdPostulacionAlumnos,
    int IdPostulacionAlumnoRequisitoModalidad,
    int IdResponsableEvidencias,
    int IdRequisitos,
    string NombreRequisito,
    string IdAlumno,
    string NombreAlumno,
    string CedulaAlumno,
    string Carrera,
    string Modalidad,
    string? EstadoEvaluacion,
    string? Observaciones,
    int? IdAdjuntosImagenes,
    string? NombreArchivoAdjunto,
    string? RutaArchivoAdjunto,
    bool Aprobado
);

public sealed record EvaluarRequisitoDocenteDto(
    int IdPostulacionAlumnoRequisitoModalidad,
    int IdResponsableEvidencias,
    bool Aprobado,
    string? Observaciones,
    int? IdAdjuntosImagenes = null
);
