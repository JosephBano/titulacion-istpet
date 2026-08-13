using System;
using System.Collections.Generic;

namespace TitulacionIstpet.Domain.Entities;

public partial class FechasHorario
{
    public int IdFecha { get; set; }

    public DateOnly? Fecha { get; set; }

    public bool? Finsemana { get; set; }

    public string? Dia { get; set; }

    public virtual ICollection<CondAcadSesione> CondAcadSesiones { get; set; } = new List<CondAcadSesione>();

    public virtual ICollection<HorarioDetalle> HorarioDetalles { get; set; } = new List<HorarioDetalle>();
}
