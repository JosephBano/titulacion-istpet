using System;
using System.Collections.Generic;

namespace TitulacionIstpet.Domain.Entities;

public partial class SectoresEmpresa
{
    public int IdsectoresEmpresas { get; set; }

    public string? NombreSector { get; set; }

    public string? CodigoSector { get; set; }

    public virtual ICollection<Empresa> Empresas { get; set; } = new List<Empresa>();
}
