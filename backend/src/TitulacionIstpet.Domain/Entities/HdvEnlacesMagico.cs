using System;
using System.Collections.Generic;

namespace TitulacionIstpet.Domain.Entities;

public partial class HdvEnlacesMagico
{
    public int IdHdvEnlacesMagicos { get; set; }

    public string IdProfesor { get; set; } = null!;

    public string Token { get; set; } = null!;

    public DateTime FechaExpiracion { get; set; }

    /// <summary>
    /// Pendiente, Utilizado, Expirado
    /// </summary>
    public string Estado { get; set; } = null!;

    public virtual ICollection<HdvSolicitudesActualizacion> HdvSolicitudesActualizacions { get; set; } = [];
}
