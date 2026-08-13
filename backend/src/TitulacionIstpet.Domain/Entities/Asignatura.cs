using System;
using System.Collections.Generic;

namespace TitulacionIstpet.Domain.Entities;

public partial class Asignatura
{
    public int IdAsignatura { get; set; }

    public string? Asignatura1 { get; set; }

    public bool? Anulada { get; set; }

    public string? Codigo { get; set; }

    public bool? ExtraCurricular { get; set; }

    public virtual ICollection<Calificacione> Calificaciones { get; set; } = [];

    public virtual ICollection<ContratosAsignatura> ContratosAsignaturas { get; set; } = [];

    public virtual ICollection<Detallemalla> Detallemallas { get; set; } = [];

    public virtual ICollection<Prerequisito> Prerequisitos { get; set; } = [];

    public virtual ICollection<Solicitudescalificacione> Solicitudescalificaciones { get; set; } = [];
}
