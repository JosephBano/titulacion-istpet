using System;
using System.Collections.Generic;

namespace TitulacionIstpet.Domain.Entities;

public partial class ModalidadesOferta
{
    public int IdmodalidadesOfertas { get; set; }

    public string? TipoModalidad { get; set; }

    public virtual ICollection<DetallesOferta> DetallesOferta { get; set; } = [];
}
