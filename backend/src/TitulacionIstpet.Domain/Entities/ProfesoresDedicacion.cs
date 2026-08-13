using System;
using System.Collections.Generic;

namespace TitulacionIstpet.Domain.Entities;

public partial class ProfesoresDedicacion
{
    public int IdProfesoresDedicacion { get; set; }

    public string IdProfesor { get; set; } = null!;

    public int IdDedicacionCategorias { get; set; }

    public string IdPeriodo { get; set; } = null!;

    public bool? EsActivo { get; set; }

    public virtual DedicacionCategoria IdDedicacionCategoriasNavigation { get; set; } = null!;

    public virtual Periodo IdPeriodoNavigation { get; set; } = null!;

    public virtual Profesore IdProfesorNavigation { get; set; } = null!;
}
