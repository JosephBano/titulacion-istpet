using System;
using System.Collections.Generic;

namespace TitulacionIstpet.Domain.Entities;

public partial class ProfesoresMotivoSalidum
{
    public string IdProfesor { get; set; } = null!;

    public int IdMotivoSalida { get; set; }

    public int IdContratos { get; set; }

    public string? Observacion { get; set; }

    public string? RutaArchivo { get; set; }

    public DateOnly? FechaRegistro { get; set; }

    public DateOnly? FechaSalida { get; set; }

    public virtual Contrato IdContratosNavigation { get; set; } = null!;

    public virtual MotivoSalidum IdMotivoSalidaNavigation { get; set; } = null!;

    public virtual Profesore IdProfesorNavigation { get; set; } = null!;
}
