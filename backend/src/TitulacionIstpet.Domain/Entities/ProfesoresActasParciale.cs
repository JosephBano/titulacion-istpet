using System;
using System.Collections.Generic;

namespace TitulacionIstpet.Domain.Entities;

public partial class ProfesoresActasParciale
{
    public int IdAsignacion { get; set; }

    public int IdParcial { get; set; }

    public bool? Activo { get; set; }

    public DateTime FechaGrabar { get; set; }

    public DateTime FechaModificacion { get; set; }

    public string? CodigoImpresion { get; set; }

    public bool? EntregaActa { get; set; }

    public bool? IngresaNotas { get; set; }

    public string? UsuarioGraba { get; set; }

    public bool? ActivoAtraso { get; set; }

    public DateOnly? FechaInicio { get; set; }

    public DateOnly? FechaFin { get; set; }
}
