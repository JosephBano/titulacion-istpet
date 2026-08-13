using System;
using System.Collections.Generic;

namespace TitulacionIstpet.Domain.Entities;

public partial class Usuario
{
    public int IdUsuario { get; set; }

    /// <summary>
    /// este es idSifafi\n
    /// </summary>
    public string IdSigafi { get; set; } = null!;

    public string TablaSigafi { get; set; } = null!;

    public string? Nombre { get; set; }

    public string Contrasenia { get; set; } = null!;

    public bool Activo { get; set; }

    public bool Administrador { get; set; }

    public string? EmailInstitucional { get; set; }

    public bool EmailValidado { get; set; }

    public string? HashEmailToken { get; set; }

    public DateTime? FechaEmailValidacion { get; set; }

    public virtual ICollection<BienApoyoFinanciero> BienApoyoFinancieros { get; set; } = new List<BienApoyoFinanciero>();

    public virtual ICollection<BienCasoDesarrollo> BienCasoDesarrollos { get; set; } = new List<BienCasoDesarrollo>();

    public virtual ICollection<BienCasoRequerimiento> BienCasoRequerimientos { get; set; } = new List<BienCasoRequerimiento>();

    public virtual ICollection<BienCaso> BienCasos { get; set; } = new List<BienCaso>();

    public virtual ICollection<BienPostulacionRequisitosBeca> BienPostulacionRequisitosBecas { get; set; } = new List<BienPostulacionRequisitosBeca>();

    public virtual ICollection<BienPostulacionesBeca> BienPostulacionesBecas { get; set; } = new List<BienPostulacionesBeca>();

    public virtual ICollection<BienResolucionesTribunale> BienResolucionesTribunales { get; set; } = new List<BienResolucionesTribunale>();

    public virtual BienTribunal? BienTribunal { get; set; }

    public virtual ICollection<BienUsuarioCaso> BienUsuarioCasos { get; set; } = new List<BienUsuarioCaso>();

    public virtual ICollection<GestPasswordReset> GestPasswordResets { get; set; } = new List<GestPasswordReset>();

    public virtual ICollection<KardexVacacione> KardexVacaciones { get; set; } = new List<KardexVacacione>();

    public virtual ICollection<RbacRefreshToken> RbacRefreshTokens { get; set; } = new List<RbacRefreshToken>();

    public virtual ICollection<RbacUsuarioRol> RbacUsuarioRols { get; set; } = new List<RbacUsuarioRol>();

    public virtual ICollection<SolicitudesLicencia> SolicitudesLicencia { get; set; } = new List<SolicitudesLicencia>();
}
