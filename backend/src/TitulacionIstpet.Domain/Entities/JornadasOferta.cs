using System;
using System.Collections.Generic;

namespace TitulacionIstpet.Domain.Entities;

public partial class JornadasOferta
{
    public int IdjornadasOfertas { get; set; }

    public string? TipoJornada { get; set; }

    public virtual ICollection<DetallesOferta> DetallesOferta { get; set; } = [];
}
