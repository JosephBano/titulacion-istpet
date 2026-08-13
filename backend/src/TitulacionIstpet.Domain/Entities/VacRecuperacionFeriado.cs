using System;
using System.Collections.Generic;

namespace TitulacionIstpet.Domain.Entities;

public partial class VacRecuperacionFeriado
{
    public int IdRecuperacionFeriado { get; set; }

    public int IdDiasEspeciales { get; set; }

    public string IdProfesor { get; set; } = null!;

    public DateOnly FechaRecuperacion { get; set; }

    public bool Completado { get; set; }

    public string? Observacion { get; set; }

    public int? RegistradoPorId { get; set; }
}
