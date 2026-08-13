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

    public virtual ICollection<Calificacione> Calificaciones { get; set; } = new List<Calificacione>();

    public virtual ICollection<ContratosAsignatura> ContratosAsignaturas { get; set; } = new List<ContratosAsignatura>();

    public virtual ICollection<Detallemalla> Detallemallas { get; set; } = new List<Detallemalla>();

    public virtual ICollection<Prerequisito> Prerequisitos { get; set; } = new List<Prerequisito>();

    public virtual ICollection<Solicitudescalificacione> Solicitudescalificaciones { get; set; } = new List<Solicitudescalificacione>();
}
