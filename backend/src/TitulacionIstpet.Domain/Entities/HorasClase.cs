using System;
using System.Collections.Generic;

namespace TitulacionIstpet.Domain.Entities;

public partial class HorasClase
{
    public int Idhora { get; set; }

    public int? IdSeccion { get; set; }

    public int? IdCarrera { get; set; }

    public string? HoraInicio { get; set; }

    public string? HoraFin { get; set; }

    public int? Minutos { get; set; }

    public int? NumeroHora { get; set; }

    public string? Tipo { get; set; }

    public bool? Activo { get; set; }

    public virtual ICollection<HorarioDetalle> HorarioDetalles { get; set; } = new List<HorarioDetalle>();
}
