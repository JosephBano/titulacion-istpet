namespace TitulacionIstpet.Application.DTOs.Egresados;

public sealed record EstudiantePendienteEgresoDto(
    string IdAlumno,
    string Nombres,
    string Apellidos,
    string NombreCompleto,
    string? Email,
    string? Celular,
    int IdCarrera,
    string Carrera,
    int IdMalla,
    string Malla,
    int? IdModalidad,
    string? Modalidad,
    string UltimoPeriodo,
    int NivelesAprobados,
    int TotalNivelesMalla,
    int MateriasAprobadas,
    int TotalMateriasMalla,
    decimal PromedioGeneral,
    bool EsEgresadoTitulado,
    bool TienePostulacionTitulacion,
    string? EstadoPostulacion
);

public sealed record FiltroPendientesEgresoRequestDto(
    string? IdPeriodo = null,
    int? IdCarrera = null,
    int? IdModalidad = null,
    string? CedulaOrNombre = null,
    int Pagina = 1,
    int TamanoPagina = 10
);

public sealed record PagedResultDto<T>(
    IReadOnlyList<T> Items,
    int TotalRegistros,
    int Pagina,
    int TamanoPagina,
    int TotalPaginas
);

public sealed record ExpedientePeriodoCursadoDto(
    string IdPeriodo,
    string NombrePeriodo,
    int IdNivel,
    string? NombreNivel,
    int? Orden,
    string? Paralelo,
    DateTime? FechaMatricula
);

public sealed record ExpedienteAsignaturaDto(
    int IdAsignatura,
    string NombreAsignatura,
    int? Nivel,
    string? NombreNivel,
    int? Creditos,
    int? Horas,
    string? IdPeriodo,
    decimal? Ef1,
    decimal? Ep1,
    decimal? Nota1,
    decimal? Ef2,
    decimal? Ep2,
    decimal? Nota2,
    decimal? Examen,
    decimal? PromedioFinal,
    decimal? NotaFinal,
    bool Aprobado,
    string? Observacion
);

public sealed record ExpedienteAcademicoDto(
    string IdAlumno,
    string Nombres,
    string Apellidos,
    string NombreCompleto,
    string? Email,
    string? EmailInstitucional,
    string? Celular,
    string? Telefono,
    string? Direccion,
    string? CiudadResidencia,
    int IdCarrera,
    string Carrera,
    int IdMalla,
    string Malla,
    int? IdModalidad,
    string? Modalidad,
    int NivelesAprobados,
    int TotalNivelesMalla,
    int MateriasAprobadas,
    int TotalMateriasMalla,
    decimal PromedioGeneral,
    bool EsEgresadoTitulado,
    string? NumeroActaGrado,
    DateOnly? FechaActaGrado,
    IReadOnlyList<ExpedientePeriodoCursadoDto> PeriodosCursados,
    IReadOnlyList<ExpedienteAsignaturaDto> Asignaturas
);
