namespace TitulacionIstpet.Application.DTOs.Actores;

public sealed record AlumnoResponseDto(
    string IdAlumno,
    string NombresCompletos,
    string PrimerNombre,
    string SegundoNombre,
    string ApellidoPaterno,
    string ApellidoMaterno,
    string EmailInstitucional,
    string EmailPersonal,
    string Telefono,
    string Celular,
    string Direccion
);

public sealed record ProfesorResponseDto(
    string IdProfesor,
    string NombresCompletos,
    string Nombres,
    string Apellidos,
    string Titulo,
    string Abreviatura,
    string EmailInstitucional,
    string Celular,
    bool Activo
);

public sealed record MatriculaResponseDto(
    int IdMatricula,
    string IdAlumno,
    string NombreAlumno,
    int? IdCarrera,
    string NombreCarrera,
    string IdPeriodo,
    int? IdNivel,
    int? IdModalidad,
    string NombreModalidad
);

public sealed record AptitudTitulacionResponseDto(
    string IdAlumno,
    string NombreAlumno,
    string NombreCarrera,
    string IdPeriodo,
    bool TieneMatriculaVigente,
    bool EsAptoTitulacion,
    string MensajeEstado
);
