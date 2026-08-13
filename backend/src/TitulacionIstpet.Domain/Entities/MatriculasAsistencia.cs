using System;
using System.Collections.Generic;

namespace TitulacionIstpet.Domain.Entities;

public partial class MatriculasAsistencia
{
    public int IdMatricula { get; set; }

    public int IdFecha { get; set; }

    public bool? NoAsiste { get; set; }

    public bool? Atraso { get; set; }

    public string? Observacion { get; set; }

    public string? Usuario { get; set; }

    public DateTime FechaCreacion { get; set; }

    public DateTime FechaActualizacion { get; set; }

    public string? UsuarioActualiza { get; set; }
}
