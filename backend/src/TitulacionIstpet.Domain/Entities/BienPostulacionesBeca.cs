using System;
using System.Collections.Generic;

namespace TitulacionIstpet.Domain.Entities;

public partial class BienPostulacionesBeca
{
    public int IdPostulacionesBecas { get; set; }

    public int? IdConvocatoriasBecas { get; set; }

    public int? IdMotivosBeca { get; set; }

    public int? IdUsuarioBienestar { get; set; }

    public int IdMatricula { get; set; }

    public DateOnly FechaRegistro { get; set; }

    public string EstadoBienestar { get; set; } = null!;

    public string? ObservacionBienestar { get; set; }

    public bool EsActivo { get; set; }

    public DateTime? FechaValidacionBienestar { get; set; }

    public DateTime? FechaActualizado { get; set; }

    public virtual ICollection<BienPostulacionRequisitosBeca> BienPostulacionRequisitosBecas { get; set; } = new List<BienPostulacionRequisitosBeca>();

    public virtual ICollection<BienResolucionesTribunale> BienResolucionesTribunales { get; set; } = new List<BienResolucionesTribunale>();

    public virtual BienConvocatoriasBeca? IdConvocatoriasBecasNavigation { get; set; }

    public virtual Matricula IdMatriculaNavigation { get; set; } = null!;

    public virtual BienMotivosBeca? IdMotivosBecaNavigation { get; set; }

    public virtual Usuario? IdUsuarioBienestarNavigation { get; set; }
}
