using System;
using System.Collections.Generic;

namespace TitulacionIstpet.Domain.Entities;

public partial class ModalidadesCarrera
{
    public int IdModalidadCarrera { get; set; }

    public int IdCarrera { get; set; }

    public int IdModalidad { get; set; }

    public bool? EsActivo { get; set; }

    public virtual Carrera IdCarreraNavigation { get; set; } = null!;

    public virtual Modalidade IdModalidadNavigation { get; set; } = null!;
}
