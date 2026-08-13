using System;
using System.Collections.Generic;

namespace TitulacionIstpet.Domain.Entities;

public partial class EmpresasContacto
{
    public int IdempresasContactos { get; set; }

    public string Idempresa { get; set; } = null!;

    public int IdtipoContacto { get; set; }

    public string? Valor { get; set; }

    public DateOnly? FechaCreacion { get; set; }

    public DateOnly? FechaModificacion { get; set; }

    public virtual Empresa IdempresaNavigation { get; set; } = null!;

    public virtual TipoContacto IdtipoContactoNavigation { get; set; } = null!;
}
