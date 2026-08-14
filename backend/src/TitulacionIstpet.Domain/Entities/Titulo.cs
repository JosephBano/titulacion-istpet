using System;
using System.Collections.Generic;

namespace TitulacionIstpet.Domain.Entities;

public partial class Titulo
{
    public int IdTitulo { get; set; }

    public string? Titulo1 { get; set; }

    public string? TituloFemenino { get; set; }

    public int? NivelInicial { get; set; }

    public int? NivelFinal { get; set; }

    public int? IdCarrera { get; set; }

    public bool? TienePracticas { get; set; }

    public int? CreditosPracticas { get; set; }

    public bool? TieneTitulacion { get; set; }

    public int? CreditosTitulacion { get; set; }
}
