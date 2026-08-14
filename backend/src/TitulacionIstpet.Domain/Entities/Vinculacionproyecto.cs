using System;
using System.Collections.Generic;

namespace TitulacionIstpet.Domain.Entities;

public partial class Vinculacionproyecto
{
    public int IdProyectoVinculacion { get; set; }

    public int? IdProgramaVinculacion { get; set; }

    public string? Proyecto { get; set; }

    public int? IdCampoDetalladoUnesco { get; set; }

    public int? IdlineaAsccion { get; set; }

    public bool? EsAsistenciaComunitaria { get; set; }

    public bool? EsEducacionContinua { get; set; }

    public string? TiempoEstimado { get; set; }

    public string? ResumenEjecutivo { get; set; }

    public string? Antecedentes { get; set; }

    public string? AlcanceTerritorial { get; set; }

    public string? Metodologia { get; set; }

    public string? Impacto { get; set; }

    public string? Innovacion { get; set; }

    public string? HabilidadesDescripcion { get; set; }

    public string? IdProfesor { get; set; }

    public bool? Activo { get; set; }

    public int? IdPoblacionDirecta { get; set; }

    public int? IdPoblacionIndirecta { get; set; }

    public int? IdPoblacionExterna { get; set; }

    public string? Biografia { get; set; }

    public virtual CampoDetalladoUnesco? IdCampoDetalladoUnescoNavigation { get; set; }

    public virtual Vinculacionlineasaccion? IdlineaAsccionNavigation { get; set; }

    public virtual ICollection<Vinculacionproyectosalumno> Vinculacionproyectosalumnos { get; set; } = [];

    public virtual ICollection<Vinculacionproyectoscarrera> Vinculacionproyectoscarreras { get; set; } = [];

    public virtual ICollection<Vinculacionproyectoscarrerasdetalle> Vinculacionproyectoscarrerasdetalles { get; set; } = [];

    public virtual ICollection<Vinculacionproyectoscronograma> Vinculacionproyectoscronogramas { get; set; } = [];

    public virtual ICollection<Vinculacionproyectosentidade> Vinculacionproyectosentidades { get; set; } = [];

    public virtual ICollection<Vinculacionproyectoshabilidadesblanda> Vinculacionproyectoshabilidadesblanda { get; set; } = [];

    public virtual ICollection<Vinculacionproyectosimpacto> Vinculacionproyectosimpactos { get; set; } = [];

    public virtual ICollection<Vinculacionproyectosmateriale> Vinculacionproyectosmateriales { get; set; } = [];

    public virtual ICollection<Vinculacionproyectosobjetivo> Vinculacionproyectosobjetivos { get; set; } = [];

    public virtual ICollection<Vinculacionproyectosobjetivosoportunidade> Vinculacionproyectosobjetivosoportunidades { get; set; } = [];

    public virtual ICollection<Vinculacionproyectosobjetivospedi> Vinculacionproyectosobjetivospedis { get; set; } = [];

    public virtual ICollection<Vinculacionproyectosperiodo> Vinculacionproyectosperiodos { get; set; } = [];

    public virtual ICollection<Vinculacionproyectosplanesaprendizaje> Vinculacionproyectosplanesaprendizajes { get; set; } = [];

    public virtual ICollection<Vinculacionproyectosplantrabajo> Vinculacionproyectosplantrabajos { get; set; } = [];

    public virtual ICollection<Vinculacionproyectospoblacione> Vinculacionproyectospoblaciones { get; set; } = [];

    public virtual ICollection<Vinculacionproyectospresupuesto> Vinculacionproyectospresupuestos { get; set; } = [];

    public virtual ICollection<Vinculacionproyectosprofesore> Vinculacionproyectosprofesores { get; set; } = [];

    public virtual ICollection<Vinculacionproyectosresponsable> Vinculacionproyectosresponsables { get; set; } = [];

    public virtual ICollection<Vinculacionproyectosresultadosaprendizaje> Vinculacionproyectosresultadosaprendizajes { get; set; } = [];
}
