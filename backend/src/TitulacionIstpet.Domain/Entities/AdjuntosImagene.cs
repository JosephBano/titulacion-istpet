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

    public virtual ICollection<BienCasoDesarrolloDoc> BienCasoDesarrolloDocs { get; set; } = new List<BienCasoDesarrolloDoc>();

    public virtual ICollection<BienCasoRequerimiento> BienCasoRequerimientos { get; set; } = new List<BienCasoRequerimiento>();

    public virtual ICollection<BienMotivoApertura> BienMotivoAperturas { get; set; } = new List<BienMotivoApertura>();

    public virtual ICollection<BienPostulacionRequisitosBeca> BienPostulacionRequisitosBecas { get; set; } = new List<BienPostulacionRequisitosBeca>();

    public virtual ICollection<CarrerasAdjunto> CarrerasAdjuntos { get; set; } = new List<CarrerasAdjunto>();

    public virtual ICollection<PlantillaContrato> PlantillaContratoIdFondoNavigations { get; set; } = new List<PlantillaContrato>();

    public virtual ICollection<PlantillaContrato> PlantillaContratoIdSelloNavigations { get; set; } = new List<PlantillaContrato>();
}
