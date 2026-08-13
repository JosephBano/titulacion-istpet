namespace TitulacionIstpet.Domain.Auth;

/// <summary>
/// Identidad de este sistema dentro del RBAC compartido de SIGAFI.
///
/// La base de datos es comun a varios sistemas del ISTPET, asi que estas dos
/// constantes son la frontera: definen que roles y que permisos pertenecen a
/// titulacion y cuales son de otro sistema y deben ignorarse.
/// </summary>
public static class RbacTitulacion
{
    /// <summary>Valor de <c>rbac_sistemas.codigo</c> que identifica a titulacion.</summary>
    public const string Codigo = "titl";

    /// <summary>Prefijo obligatorio de <c>rbac_roles.codigo_rol</c> para roles de titulacion.</summary>
    public const string PrefijoRol = "titul_";

    /// <summary>
    /// Un rol pertenece a titulacion si su codigo empieza por <see cref="PrefijoRol"/>.
    ///
    /// Es solo la primera de dos guardas: el codigo del rol es una convencion de nombres
    /// y podria estar mal cargado. La segunda guarda —que el rol este vinculado a un
    /// modulo del sistema 'titl'— vive en <see cref="AccesoTitulacion"/>. Ninguna de las
    /// dos alcanza por si sola.
    /// </summary>
    public static bool EsRolDeTitulacion(string? codigoRol)
    {
        if (string.IsNullOrWhiteSpace(codigoRol))
        {
            return false;
        }

        // Ordinal e insensible a mayusculas: MySQL compara sin distinguir caja con las
        // colaciones _ci habituales, asi que 'TITUL_ADMIN' y 'titul_admin' son el mismo
        // rol para la base. Tratarlos distinto aqui abriria una discrepancia silenciosa.
        return codigoRol.Trim().StartsWith(PrefijoRol, StringComparison.OrdinalIgnoreCase);
    }
}
