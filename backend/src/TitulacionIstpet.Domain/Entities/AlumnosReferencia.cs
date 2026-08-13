using System;
using System.Collections.Generic;

namespace TitulacionIstpet.Domain.Entities;

public partial class AlumnosReferencia
{
    public int IdalumnosReferencias { get; set; }

    public string IdAlumno { get; set; } = null!;

    public string? NombresReferencia { get; set; }

    public string? Contacto { get; set; }

    public string? ReferenciaEmpresa { get; set; }

    public string? Relacion { get; set; }

    public DateOnly? FechaCreacion { get; set; }

    public DateOnly? FechaModificacion { get; set; }
}
