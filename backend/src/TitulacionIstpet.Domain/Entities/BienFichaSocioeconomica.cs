using System;
using System.Collections.Generic;

namespace TitulacionIstpet.Domain.Entities;

public partial class BienFichaSocioeconomica
{
    public int IdFichaSocioEconomica { get; set; }

    public int IdTipoVivienda { get; set; }

    public int MiembrosHogar { get; set; }

    public int MiembrosAdulto { get; set; }

    public int MiembrosNinos { get; set; }

    public DateTime FechaRegistro { get; set; }

    public bool EstudiaOtroSitio { get; set; }

    public int EspaciosFisicosVivienda { get; set; }

    public int NumeroDormitorioriosVivienda { get; set; }

    public string IdAlumno { get; set; } = null!;

    public bool? RequiereActualizacion { get; set; }

    public string? RazonActualizacion { get; set; }

    public string? UltimaFechaActualizacion { get; set; }

    public virtual BienDatosEconomico? BienDatosEconomico { get; set; }

    public virtual ICollection<BienDetalleViviendum> BienDetalleVivienda { get; set; } = new List<BienDetalleViviendum>();

    public virtual ICollection<BienServiciosFicha> BienServiciosFichas { get; set; } = new List<BienServiciosFicha>();

    public virtual Alumno IdAlumnoNavigation { get; set; } = null!;

    public virtual BienTipoViviendum IdTipoViviendaNavigation { get; set; } = null!;
}
