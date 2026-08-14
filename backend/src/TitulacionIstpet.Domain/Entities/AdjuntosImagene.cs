using System;
using System.Collections.Generic;

namespace TitulacionIstpet.Domain.Entities;

public partial class AdjuntosImagene
{
    public int IdAdjuntosImagenes { get; set; }

    public string? NombreArchivos { get; set; }

    public string? Extension { get; set; }

    public string? MimeTypes { get; set; }

    public int? TamanioBytes { get; set; }

    public string? Ruta { get; set; }

    public virtual ICollection<BienCasoDesarrolloDoc> BienCasoDesarrolloDocs { get; set; } = [];

    public virtual ICollection<BienCasoRequerimiento> BienCasoRequerimientos { get; set; } = [];

    public virtual ICollection<BienMotivoApertura> BienMotivoAperturas { get; set; } = [];

    public virtual ICollection<BienPostulacionRequisitosBeca> BienPostulacionRequisitosBecas { get; set; } = [];

    public virtual ICollection<CarrerasAdjunto> CarrerasAdjuntos { get; set; } = [];

    public virtual ICollection<PlantillaContrato> PlantillaContratoIdFondoNavigations { get; set; } = [];

    public virtual ICollection<PlantillaContrato> PlantillaContratoIdSelloNavigations { get; set; } = [];
}
