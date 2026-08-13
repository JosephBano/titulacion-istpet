using System;
using System.Collections.Generic;

namespace TitulacionIstpet.Domain.Entities;

public partial class AsignacionInstructoresVehiculo
{
    public int IdAsignacion { get; set; }

    public int IdVehiculo { get; set; }

    public string IdProfesor { get; set; } = null!;

    public DateOnly? FechaAsignacion { get; set; }

    public DateOnly? FechaSalidad { get; set; }

    public bool? Activo { get; set; }

    public string? UsuarioAsigna { get; set; }

    public string? UsuarioDesactiva { get; set; }

    public string? Observacion { get; set; }
}
