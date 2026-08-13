using System;
using System.Collections.Generic;

namespace TitulacionIstpet.Domain.Entities;

public partial class Carrera
{
    public int IdCarrera { get; set; }

    public string? Carrera1 { get; set; }

    public DateOnly? FechaCreacion { get; set; }

    public bool? Activa { get; set; }

    public string? DirectorCarrera { get; set; }

    public int? NumeroCreditos { get; set; }

    public int? OrdenCarrera { get; set; }

    public int? NumeroAlumnos { get; set; }

    public bool? RevisaArrastres { get; set; }

    public string? CodigoCases { get; set; }

    public string? AliasCarrera { get; set; }

    public bool? BolsaEmpleo { get; set; }

    public bool? EsInstituto { get; set; }

    public virtual ICollection<CarrerasAdjunto> CarrerasAdjuntos { get; set; } = [];

    public virtual ICollection<Curso> Cursos { get; set; } = [];

    public virtual ICollection<Espacio> Espacios { get; set; } = [];

    public virtual ICollection<FechasPagosCuota> FechasPagosCuota { get; set; } = [];

    public virtual ICollection<Malla> Mallas { get; set; } = [];

    public virtual ICollection<ModalidadesCarrera> ModalidadesCarreras { get; set; } = [];

    public virtual ICollection<OfertasCarrera> OfertasCarreras { get; set; } = [];

    public virtual ICollection<ProfesoresCarrerasPeriodo> ProfesoresCarrerasPeriodos { get; set; } = [];

    public virtual ICollection<Vinculacionproyectoscarrera> Vinculacionproyectoscarreras { get; set; } = [];
}
