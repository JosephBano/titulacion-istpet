namespace TitulacionIstpet.Application.DTOs.Actores;

public sealed record AlumnoAptoDto(
    string IdAlumno,
    string NombresCompletos,
    string EmailInstitucional,
    string Celular,
    int? IdCarrera,
    string Carrera,
    int? IdModalidad,
    string Modalidad,
    string IdPeriodo,
    string EstadoTitulacion
);

public sealed record GraduadoHistoricoDto(
    string IdAlumno,
    string NombresCompletos,
    int IdTitulo,
    string NumeroActa,
    DateOnly? FechaActa,
    decimal? NotaFinal,
    decimal? PromedioEstudios,
    string TituloTesis
);
