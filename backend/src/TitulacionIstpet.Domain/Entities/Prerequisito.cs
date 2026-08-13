using System;
using System.Collections.Generic;

namespace TitulacionIstpet.Domain.Entities;

public partial class Prerequisito
{
    public int IdDetalleMalla { get; set; }

    public int IdAsignatura { get; set; }

    public bool? Activa { get; set; }

    public virtual Asignatura IdAsignaturaNavigation { get; set; } = null!;

    public virtual Detallemalla IdDetalleMallaNavigation { get; set; } = null!;
}
