using System;
using System.Collections.Generic;

namespace TitulacionIstpet.Domain.Entities;

public partial class DedicacionCategoria
{
    public int IdDedicacionCategorias { get; set; }

    public int IdDedicacion { get; set; }

    public int IdEscalafon { get; set; }

    public int? HorasMinimas { get; set; }

    public int? HorasMaximas { get; set; }

    public bool? EsActivo { get; set; }

    public virtual ICollection<Contrato> Contratos { get; set; } = [];

    public virtual Dedicacion IdDedicacionNavigation { get; set; } = null!;

    public virtual Escalafon IdEscalafonNavigation { get; set; } = null!;

    public virtual ICollection<ProfesoresDedicacion> ProfesoresDedicacions { get; set; } = [];
}
