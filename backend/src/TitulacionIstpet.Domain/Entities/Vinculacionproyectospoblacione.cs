using System;
using System.Collections.Generic;

namespace TitulacionIstpet.Domain.Entities;

public partial class Vinculacionproyectospoblacione
{
    public int IdProyectosPoblaciones { get; set; }

    public int? IdProyectoVinculacion { get; set; }

    public string? Nombre { get; set; }

    public string? Direccion { get; set; }

    public string? Telefono { get; set; }

    public string? Email { get; set; }

    public string? Contacto { get; set; }

    public DateTime? FechaRegistro { get; set; }

    public bool Activo { get; set; }

    public virtual Vinculacionproyecto? IdProyectoVinculacionNavigation { get; set; }
}
