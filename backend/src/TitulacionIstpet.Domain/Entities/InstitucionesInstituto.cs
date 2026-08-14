using System;
using System.Collections.Generic;

namespace TitulacionIstpet.Domain.Entities;

public partial class InstitucionesInstituto
{
    public int IdInstitucionesInstituto { get; set; }

    public string? Nombre { get; set; }

    public string? Ruc { get; set; }

    public string? Ubicado { get; set; }

    public string? Representante { get; set; }

    public string? CedulaRepresentante { get; set; }

    public virtual ICollection<Contrato> Contratos { get; set; } = [];

    public virtual ICollection<PlantillaContrato> PlantillaContratos { get; set; } = [];
}
