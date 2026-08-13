using System;
using System.Collections.Generic;

namespace TitulacionIstpet.Domain.Entities;

public partial class CampoDetalladoUnesco
{
    public int IdCampoDetalladoUnesco { get; set; }

    public int? IdCampospecificoUnesco { get; set; }

    public string? NombreDetallado { get; set; }

    public string? CodigoDetallado { get; set; }

    public bool? Activo { get; set; }

    public virtual CampoEspecificoUnesco? IdCampospecificoUnescoNavigation { get; set; }

    public virtual ICollection<TitulosEnCurso> TitulosEnCursos { get; set; } = new List<TitulosEnCurso>();

    public virtual ICollection<TitulosProfesore> TitulosProfesores { get; set; } = new List<TitulosProfesore>();

    public virtual ICollection<Vinculacionproyecto> Vinculacionproyectos { get; set; } = new List<Vinculacionproyecto>();
}
