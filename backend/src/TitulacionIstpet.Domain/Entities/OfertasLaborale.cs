using System;
using System.Collections.Generic;

namespace TitulacionIstpet.Domain.Entities;

public partial class OfertasLaborale
{
    public int IdofertasLaborales { get; set; }

    public string Idempresa { get; set; } = null!;

    public int Iddepartamentos { get; set; }

    public int IdcargosOfertas { get; set; }

    public string? Provincia { get; set; }

    public string? Ciudad { get; set; }

    public string? Ubicacion { get; set; }

    public int IdtiposOfertas { get; set; }

    public string? ExperienciaRequerida { get; set; }

    public int? Vacantes { get; set; }

    public string? Estado { get; set; }

    public DateTime? FechaPublicacion { get; set; }

    public DateTime? FechaModificacion { get; set; }

    public DateTime? FechaCierre { get; set; }

    public decimal? Salario { get; set; }

    public string? EnlaceOriginal { get; set; }

    public bool? EsActivo { get; set; }

    public virtual ICollection<DetallesOferta> DetallesOferta { get; set; } = new List<DetallesOferta>();

    public virtual ICollection<HabilidadesRequerida> HabilidadesRequerida { get; set; } = new List<HabilidadesRequerida>();

    public virtual CargosOferta IdcargosOfertasNavigation { get; set; } = null!;

    public virtual Departamento IddepartamentosNavigation { get; set; } = null!;

    public virtual Empresa IdempresaNavigation { get; set; } = null!;

    public virtual TiposOferta IdtiposOfertasNavigation { get; set; } = null!;

    public virtual ICollection<OfertasCarrera> OfertasCarreras { get; set; } = new List<OfertasCarrera>();

    public virtual ICollection<OfertasRequisito> OfertasRequisitos { get; set; } = new List<OfertasRequisito>();

    public virtual ICollection<Postulacione> Postulaciones { get; set; } = new List<Postulacione>();
}
