using System;
using System.Collections.Generic;

namespace TitulacionIstpet.Domain.Entities;

public partial class DetallesOferta
{
    public int IddetallesOfertas { get; set; }

    public int IdofertasLaborales { get; set; }

    public int IdjornadasOfertas { get; set; }

    public int IdmodalidadesOfertas { get; set; }

    public virtual JornadasOferta IdjornadasOfertasNavigation { get; set; } = null!;

    public virtual ModalidadesOferta IdmodalidadesOfertasNavigation { get; set; } = null!;

    public virtual OfertasLaborale IdofertasLaboralesNavigation { get; set; } = null!;
}
