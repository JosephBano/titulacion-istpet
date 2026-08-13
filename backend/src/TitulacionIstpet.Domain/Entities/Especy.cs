using System;
using System.Collections.Generic;

namespace TitulacionIstpet.Domain.Entities;

public partial class Especy
{
    public int IdEspecie { get; set; }

    public string Especie { get; set; } = null!;

    public decimal Valor { get; set; }

    public int NumeroCuotas { get; set; }

    public int? Prioridad { get; set; }

    public bool? PermiteIntercalar { get; set; }

    public string? CodigoReferencia { get; set; }

    public string? Idperiodo { get; set; }

    public decimal? Extraordinaria { get; set; }

    public int? IdNivel { get; set; }

    public virtual ICollection<DetallePago> DetallePagos { get; set; } = new List<DetallePago>();
}
