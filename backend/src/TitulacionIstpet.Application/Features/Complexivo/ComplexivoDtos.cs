namespace TitulacionIstpet.Application.Features.Complexivo;

public sealed record CohorteCarreraComplexivoDto(
    int IdCohorteCarrera, int IdCohorte, int IdCarrera,
    string Cohorte, string Periodo, string Carrera, string ModalidadEstudio, bool EsActivo);

public sealed record TemaComplexivoDto(int IdTemas, int IdAsignatura, string Detalle, int HorasPorTema);

public sealed record HorarioComplexivoDto(int IdModulosHorarios, string Dias, string FranjaHoraria);

public sealed record DocenteComplexivoDto(
    int IdMallaDocenteComplexivo, string? IdProfesor, DateOnly? FechaAsignacion,
    string? CodigoAsignacion, bool EsActivo, IReadOnlyList<HorarioComplexivoDto> Horarios);

public sealed record GuardarMallaComplexivoDto(
    int IdCohorteCarrera, string Detalle, string DetalleModulo, DateOnly FechaInicio,
    DateOnly FechaFin, string Modalidad, int TotalHoras, bool EsActivo,
    IReadOnlyList<TemaComplexivoDto> Temas, IReadOnlyList<DocenteComplexivoDto> Docentes);

public sealed record MallaComplexivoDto(
    int IdMallaComplexivo, int IdModulosComplexivo, int IdCohorteCarrera,
    string Detalle, string DetalleModulo, DateOnly FechaInicio, DateOnly FechaFin,
    string Modalidad, int TotalHoras, bool EsActivo,
    IReadOnlyList<TemaComplexivoDto> Temas, IReadOnlyList<DocenteComplexivoDto> Docentes);
