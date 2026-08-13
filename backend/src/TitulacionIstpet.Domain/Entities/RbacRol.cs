using System;
using System.Collections.Generic;

namespace TitulacionIstpet.Domain.Entities;

public partial class RbacRol
{
    public int IdRol { get; set; }

    public string Nombre { get; set; } = null!;

    public string CodigoRol { get; set; } = null!;

    public bool? EsActivo { get; set; }

    public virtual ICollection<RbacRolModuloOperacion> RbacRolModuloOperacions { get; set; } = new List<RbacRolModuloOperacion>();

    public virtual ICollection<RbacUsuarioRol> RbacUsuarioRols { get; set; } = new List<RbacUsuarioRol>();
}
