using System;
using System.Collections.Generic;

namespace TitulacionIstpet.Domain.Entities;

public partial class Contrato
{
    public int IdContratos { get; set; }

    public int IdInstitucionesInstituto { get; set; }

    public string IdProfesor { get; set; } = null!;

    public int IdDedicacionCategorias { get; set; }

    public int? IdTiposContratos { get; set; }

    public int? IdRelacionIes { get; set; }

    public int? Iddepartamentos { get; set; }

    public int? IdCargoInstituto { get; set; }

    public string? NumeroContrato { get; set; }

    public bool? EsAdendum { get; set; }

    public string? ContratoVinculado { get; set; }

    public DateOnly? FechaRegistro { get; set; }

    public DateOnly? FechaInicio { get; set; }

    public DateOnly? FechaFinal { get; set; }

    public bool? EsActivo { get; set; }

    public string? ArchivoContrato { get; set; }

    public string? ArchivoLegalizado { get; set; }

    public string? ArchivoFiniquito { get; set; }

    public string? ArchivoLegalizadoSalida { get; set; }

    public bool? IngresoConcurso { get; set; }

    public string UsuarioCreo { get; set; } = null!;

    public string? UsuariosModifico { get; set; }

    public DateOnly? FechaModifico { get; set; }

    public DateOnly? Reingreso { get; set; }

    public virtual ICollection<ContratosAsignatura> ContratosAsignaturas { get; set; } = [];

    public virtual ICollection<ContratosFactura> ContratosFacturas { get; set; } = [];

    public virtual ICollection<ExtrasContrato> ExtrasContratos { get; set; } = [];

    public virtual DedicacionCategoria IdDedicacionCategoriasNavigation { get; set; } = null!;

    public virtual InstitucionesInstituto IdInstitucionesInstitutoNavigation { get; set; } = null!;

    public virtual Profesore IdProfesorNavigation { get; set; } = null!;

    public virtual ICollection<ProfesoresMotivoSalidum> ProfesoresMotivoSalida { get; set; } = [];

    public virtual ICollection<SueldosContrato> SueldosContratos { get; set; } = [];
}
