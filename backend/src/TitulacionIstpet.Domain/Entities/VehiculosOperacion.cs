using System;
using System.Collections.Generic;

namespace TitulacionIstpet.Domain.Entities;

public partial class VehiculosOperacion
{
    public int IdVehiculo { get; set; }

    public int? IdTipoLicencia { get; set; }

    public string? IdInstructorFijo { get; set; }

    public string? EstadoMecanico { get; set; }

    public virtual Vehiculo IdVehiculoNavigation { get; set; } = null!;
}
