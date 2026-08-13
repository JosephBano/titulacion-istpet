using System;
using System.Collections.Generic;

namespace TitulacionIstpet.Domain.Entities;

public partial class Seddautoridadescarrerasperiodo
{
    public int IdAsignacion { get; set; }

    public int? IdCarrera { get; set; }

    public string? IdPeriodo { get; set; }

    public string? IdProfesor { get; set; }

    public int? IdInstrumento { get; set; }

    public string? Designacion { get; set; }

    public bool? Activo { get; set; }
}
