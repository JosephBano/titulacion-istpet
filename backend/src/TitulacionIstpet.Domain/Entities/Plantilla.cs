using System;
using System.Collections.Generic;

namespace TitulacionIstpet.Domain.Entities;

public partial class Plantilla
{
    public int IdPlantilla { get; set; }

    public DateTime? FechaRegistro { get; set; }

    public string? Nombre { get; set; }

    public string? Archivo { get; set; }

    public string? Usuario { get; set; }

    public bool? Activo { get; set; }

    public virtual ICollection<Plantillasparametro> Plantillasparametros { get; set; } = new List<Plantillasparametro>();
}
