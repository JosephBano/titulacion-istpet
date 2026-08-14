using System;
using System.Collections.Generic;

namespace TitulacionIstpet.Domain.Entities;

public partial class HdvSolicitudesActualizacion
{
    public int IdHdvSolicitudesActualizacion { get; set; }

    public string IdProfesor { get; set; } = null!;

    public int IdHdvEnlacesMagicos { get; set; }

    public string DatosPropuestos { get; set; } = null!;

    public string RutaArchivosAdjuntos { get; set; } = null!;

    /// <summary>
    /// Pendiente, Aprobado, Rechazado
    /// 
    /// </summary>
    public string Estado { get; set; } = null!;

    public DateTime FechaSolicitud { get; set; }

    public int? RevisadoPor { get; set; }

    public DateTime FechaRevision { get; set; }

    public virtual HdvEnlacesMagico IdHdvEnlacesMagicosNavigation { get; set; } = null!;
}
