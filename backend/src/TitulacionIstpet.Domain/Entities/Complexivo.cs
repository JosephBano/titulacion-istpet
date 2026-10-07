namespace TitulacionIstpet.Domain.Entities;

public sealed class TitulModuloComplexivo
{
    public int IdModulosComplexivo { get; set; }
    public int IdCarrera { get; set; }
    public string Detalle { get; set; } = string.Empty;
    public bool EsActivo { get; set; } = true;
    public DateTime? FechaCreacion { get; set; }
    public DateTime? FechaDesactivacion { get; set; }
    public int? TotalHoras { get; set; }
    public DateOnly FechaInicio { get; set; }
    public DateOnly FechaFin { get; set; }
    public string? Modalidad { get; set; }
    public ICollection<TitulTemaComplexivo> Temas { get; set; } = new List<TitulTemaComplexivo>();
}

public sealed class TitulTemaComplexivo
{
    public int IdTemas { get; set; }
    public int IdModulosComplexivo { get; set; }
    public int IdAsignatura { get; set; }
    public string? Detalle { get; set; }
    public int? HorasPorTema { get; set; }
    public Asignaturas Asignatura { get; set; } = null!;
}

public sealed class TitulMallaComplexivo
{
    public int IdMallaComplexivo { get; set; }
    public int IdCohorteCarrera { get; set; }
    public int IdModulosComplexivo { get; set; }
    public string Detalle { get; set; } = string.Empty;
    public DateOnly FechaInicio { get; set; }
    public DateOnly FechaFin { get; set; }
    public bool EsActivo { get; set; } = true;
    public TitulModuloComplexivo Modulo { get; set; } = null!;
    public ICollection<TitulMallaDocenteComplexivo> Docentes { get; set; } = new List<TitulMallaDocenteComplexivo>();
}

public sealed class TitulMallaDocenteComplexivo
{
    public int IdMallaDocenteComplexivo { get; set; }
    public int IdMallaComplexivo { get; set; }
    public string? IdProfesor { get; set; }
    public DateOnly? FechaAsignacion { get; set; }
    public string? CodigoAsignacion { get; set; }
    public bool? EsActivo { get; set; } = true;
    public Profesores? Profesor { get; set; }
    public ICollection<TitulModuloHorarioComplexivo> Horarios { get; set; } = new List<TitulModuloHorarioComplexivo>();
}

public sealed class TitulModuloHorarioComplexivo
{
    public int IdModulosHorarios { get; set; }
    public int IdModulosProfesores { get; set; }
    public string? Dias { get; set; }
    public string? FranjaHoraria { get; set; }
}
