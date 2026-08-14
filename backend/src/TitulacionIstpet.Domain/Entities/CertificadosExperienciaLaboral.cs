using System;
using System.Collections.Generic;

namespace TitulacionIstpet.Domain.Entities;

public partial class CertificadosExperienciaLaboral
{
    public int IdcertificadosExperienciaLaboral { get; set; }

    public string IdProfesor { get; set; } = null!;

    public DateTime FechaEmision { get; set; }

    public bool GeneradoAutomaticamente { get; set; }

    public string? Ruta { get; set; }

    public bool EsActivo { get; set; }

    public virtual Profesore IdProfesorNavigation { get; set; } = null!;
}
