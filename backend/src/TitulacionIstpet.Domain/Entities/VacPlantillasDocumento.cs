using System;
using System.Collections.Generic;

namespace TitulacionIstpet.Domain.Entities;

public partial class VacPlantillasDocumento
{
    public int IdPlantilla { get; set; }

    public string Codigo { get; set; } = null!;

    public string Nombre { get; set; } = null!;

    public string Cuerpo { get; set; } = null!;

    public bool? Activo { get; set; }

    public DateTime FechaActualizacion { get; set; }
}
