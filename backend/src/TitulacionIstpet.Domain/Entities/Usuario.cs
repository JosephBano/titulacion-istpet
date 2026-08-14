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

    public virtual ICollection<BienApoyoFinanciero> BienApoyoFinancieros { get; set; } = [];

    public virtual ICollection<BienCasoDesarrollo> BienCasoDesarrollos { get; set; } = [];

    public virtual ICollection<BienCasoRequerimiento> BienCasoRequerimientos { get; set; } = [];

    public virtual ICollection<BienCaso> BienCasos { get; set; } = [];

    public virtual ICollection<BienPostulacionRequisitosBeca> BienPostulacionRequisitosBecas { get; set; } = [];

    public virtual ICollection<BienPostulacionesBeca> BienPostulacionesBecas { get; set; } = [];

    public virtual ICollection<BienResolucionesTribunale> BienResolucionesTribunales { get; set; } = [];

    public virtual BienTribunal? BienTribunal { get; set; }

    public virtual ICollection<BienUsuarioCaso> BienUsuarioCasos { get; set; } = [];

    public virtual ICollection<GestPasswordReset> GestPasswordResets { get; set; } = [];

    public virtual ICollection<KardexVacacione> KardexVacaciones { get; set; } = [];

    public virtual ICollection<RbacRefreshToken> RbacRefreshTokens { get; set; } = [];

    public virtual ICollection<RbacUsuarioRol> RbacUsuarioRols { get; set; } = [];

    public virtual ICollection<SolicitudesLicencia> SolicitudesLicencia { get; set; } = [];
}
