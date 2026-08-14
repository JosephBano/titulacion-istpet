using System;
using System.Collections.Generic;

namespace TitulacionIstpet.Domain.Entities;

public partial class VacConfigDiasExtrasExcepcione
{
    public int IdExcepcion { get; set; }

    public int IdConfig { get; set; }

    public string IdProfesor { get; set; } = null!;

    public string Tipo { get; set; } = null!;

    public virtual VacConfigDiasExtrasDepto IdConfigNavigation { get; set; } = null!;

    public virtual Profesore IdProfesorNavigation { get; set; } = null!;
}
