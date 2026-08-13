using System;
using System.Collections.Generic;

namespace TitulacionIstpet.Domain.Entities;

public partial class Empresa
{
    public string Idempresa { get; set; } = null!;

    public string? TipoDocumento { get; set; }

    public int IdsectoresEmpresas { get; set; }

    public string? NombreEmpresa { get; set; }

    public string? PaisEmpresa { get; set; }

    public string? CiudadEmpresa { get; set; }

    public string? DireccionEmpresa { get; set; }

    public string? TelefonoEmpresa { get; set; }

    public string? EmailEmpresa { get; set; }

    public string? UserEmpresa { get; set; }

    public string? Password { get; set; }

    public DateTime? FechaInscripcion { get; set; }

    public DateTime? FechaModificacion { get; set; }

    public string? EstadoVerificacion { get; set; }

    public DateOnly? FechaVerificacion { get; set; }

    public string? ComentarioVerificacion { get; set; }

    public bool? EsActivo { get; set; }

    public virtual ICollection<EmpresasContacto> EmpresasContactos { get; set; } = new List<EmpresasContacto>();

    public virtual SectoresEmpresa IdsectoresEmpresasNavigation { get; set; } = null!;

    public virtual ICollection<OfertasLaborale> OfertasLaborales { get; set; } = new List<OfertasLaborale>();
}
