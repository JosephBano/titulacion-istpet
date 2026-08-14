using System;
using System.Collections.Generic;

namespace TitulacionIstpet.Domain.Entities;

public partial class Vehiculo
{
    public int IdVehiculo { get; set; }

    public int? IdSubcategoria { get; set; }

    public string? NumeroVehiculo { get; set; }

    public string? Placa { get; set; }

    public string? Marca { get; set; }

    public int? Anio { get; set; }

    public int? IdCategoria { get; set; }

    public bool? Activo { get; set; }

    public string? Observacion { get; set; }

    public string? Chasis { get; set; }

    public string? Motor { get; set; }

    public string? Modelo { get; set; }

    public virtual VehiculosOperacion? VehiculosOperacion { get; set; }
}
