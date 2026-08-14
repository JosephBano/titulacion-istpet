using System;
using System.Collections.Generic;

namespace TitulacionIstpet.Domain.Entities;

public partial class Modalidade
{
    public int IdModalidad { get; set; }

    public string? Modalidad { get; set; }

    public string? Sufijo { get; set; }

    public string? ModalidadImpresion { get; set; }

    public virtual ICollection<FechasPagosCuota> FechasPagosCuota { get; set; } = [];

    public virtual ICollection<Matricula> Matriculas { get; set; } = [];

    public virtual ICollection<ModalidadesCarrera> ModalidadesCarreras { get; set; } = [];
}
