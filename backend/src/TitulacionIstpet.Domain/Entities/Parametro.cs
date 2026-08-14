using System;
using System.Collections.Generic;

namespace TitulacionIstpet.Domain.Entities;

public partial class Parametro
{
    public string? CodigoInstitucion { get; set; }

    public string? NombreInstitucion { get; set; }

    public string? CadenaConexion { get; set; }

    public string? NombreRector { get; set; }

    public string? ArchivoFirma { get; set; }

    public string? ArchivoSello { get; set; }

    public string? EmailSolicitudes { get; set; }

    public string? ClaveEmailSolicitudes { get; set; }

    public bool? Activo { get; set; }

    public bool? PermiteActualizacionCompleta { get; set; }
}
