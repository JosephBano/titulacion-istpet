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

    public virtual ICollection<Vinculacionproyectosalumno> Vinculacionproyectosalumnos { get; set; } = new List<Vinculacionproyectosalumno>();

    public virtual ICollection<Vinculacionproyectoscarrera> Vinculacionproyectoscarreras { get; set; } = new List<Vinculacionproyectoscarrera>();

    public virtual ICollection<Vinculacionproyectoscarrerasdetalle> Vinculacionproyectoscarrerasdetalles { get; set; } = new List<Vinculacionproyectoscarrerasdetalle>();

    public virtual ICollection<Vinculacionproyectoscronograma> Vinculacionproyectoscronogramas { get; set; } = new List<Vinculacionproyectoscronograma>();

    public virtual ICollection<Vinculacionproyectosentidade> Vinculacionproyectosentidades { get; set; } = new List<Vinculacionproyectosentidade>();

    public virtual ICollection<Vinculacionproyectoshabilidadesblanda> Vinculacionproyectoshabilidadesblanda { get; set; } = new List<Vinculacionproyectoshabilidadesblanda>();

    public virtual ICollection<Vinculacionproyectosimpacto> Vinculacionproyectosimpactos { get; set; } = new List<Vinculacionproyectosimpacto>();

    public virtual ICollection<Vinculacionproyectosmateriale> Vinculacionproyectosmateriales { get; set; } = new List<Vinculacionproyectosmateriale>();

    public virtual ICollection<Vinculacionproyectosobjetivo> Vinculacionproyectosobjetivos { get; set; } = new List<Vinculacionproyectosobjetivo>();

    public virtual ICollection<Vinculacionproyectosobjetivosoportunidade> Vinculacionproyectosobjetivosoportunidades { get; set; } = new List<Vinculacionproyectosobjetivosoportunidade>();

    public virtual ICollection<Vinculacionproyectosobjetivospedi> Vinculacionproyectosobjetivospedis { get; set; } = new List<Vinculacionproyectosobjetivospedi>();

    public virtual ICollection<Vinculacionproyectosperiodo> Vinculacionproyectosperiodos { get; set; } = new List<Vinculacionproyectosperiodo>();

    public virtual ICollection<Vinculacionproyectosplanesaprendizaje> Vinculacionproyectosplanesaprendizajes { get; set; } = new List<Vinculacionproyectosplanesaprendizaje>();

    public virtual ICollection<Vinculacionproyectosplantrabajo> Vinculacionproyectosplantrabajos { get; set; } = new List<Vinculacionproyectosplantrabajo>();

    public virtual ICollection<Vinculacionproyectospoblacione> Vinculacionproyectospoblaciones { get; set; } = new List<Vinculacionproyectospoblacione>();

    public virtual ICollection<Vinculacionproyectospresupuesto> Vinculacionproyectospresupuestos { get; set; } = new List<Vinculacionproyectospresupuesto>();

    public virtual ICollection<Vinculacionproyectosprofesore> Vinculacionproyectosprofesores { get; set; } = new List<Vinculacionproyectosprofesore>();

    public virtual ICollection<Vinculacionproyectosresponsable> Vinculacionproyectosresponsables { get; set; } = new List<Vinculacionproyectosresponsable>();

    public virtual ICollection<Vinculacionproyectosresultadosaprendizaje> Vinculacionproyectosresultadosaprendizajes { get; set; } = new List<Vinculacionproyectosresultadosaprendizaje>();
}
