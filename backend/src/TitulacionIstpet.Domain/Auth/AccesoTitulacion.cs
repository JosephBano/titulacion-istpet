using TitulacionIstpet.Domain.Entities;

namespace TitulacionIstpet.Domain.Auth;

/// <summary>
/// Decide que roles de titulacion tiene un usuario, recorriendo el grafo RBAC compartido.
///
/// Es una funcion pura sobre entidades ya cargadas: no consulta la base. Eso permite
/// testear el aislamiento entre sistemas —la parte que de verdad importa— sin MySQL.
///
/// La cadena que se recorre es:
///
///   RbacUsuarioRol -> RbacRol -> RbacRolModuloOperacion -> RbacModulosOperacione
///                            -> RbacModulo -> RbacSistema.Codigo
///
/// NULL es inactivo en TODA la cadena: asignacion, rol, permiso, modulo-operacion y
/// modulo deben tener EsActivo == true. Una columna sin valor no concede nada.
///
/// FAIL-CLOSED: si el usuario no tiene ningun rol que satisfaga AMBAS guardas, el
/// resultado es un conjunto vacio y el acceso se niega. Esto difiere a proposito del
/// AuthService de auth_global, que ante una lista vacia devuelve TODOS los sistemas
/// (ver AuthService.cs, GetUsuarioSistemasAsync). Esa rama convierte "sin permisos"
/// en "acceso total"; aqui no se replica.
/// </summary>
public static class AccesoTitulacion
{
    /// <summary>
    /// Roles de titulacion efectivos del usuario. Vacio significa sin acceso.
    /// </summary>
    /// <param name="asignaciones">
    /// Asignaciones usuario-rol del usuario, con RbacRol y su cadena de permisos cargados.
    /// </param>
    public static IReadOnlySet<string> ResolverRoles(IEnumerable<RbacUsuarioRol> asignaciones)
    {
        ArgumentNullException.ThrowIfNull(asignaciones);

        var roles = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var asignacion in asignaciones)
        {
            if (!AsignacionVigente(asignacion))
            {
                continue;
            }

            var rol = asignacion.IdRolNavigation;
            if (rol is null || !RolVigente(rol))
            {
                continue;
            }

            // Guarda 1: convencion de nombres.
            if (!RbacTitulacion.EsRolDeTitulacion(rol.CodigoRol))
            {
                continue;
            }

            // Guarda 2: vinculo relacional real con el sistema 'titl'.
            if (!ConcedePermisosEnTitulacion(rol))
            {
                continue;
            }

            roles.Add(rol.CodigoRol.Trim());
        }

        return roles;
    }

    public static bool TieneAcceso(IEnumerable<RbacUsuarioRol> asignaciones) =>
        ResolverRoles(asignaciones).Count > 0;

    /// <summary>
    /// La asignacion usuario-rol debe estar explicitamente activa.
    /// </summary>
    private static bool AsignacionVigente(RbacUsuarioRol asignacion) =>
        asignacion.EsActivo == true;

    /// <summary>
    /// El rol debe estar explicitamente activo.
    ///
    /// NOTA: esto diverge de auth_global, que acepta <c>EsActivo == 1 || null</c> y por
    /// tanto trata NULL como activo. Titulacion exige el valor explicito, de modo que un
    /// usuario cuyo unico rol tenga la columna en NULL podra entrar a los demas sistemas
    /// pero no a este. Es intencional: sin un valor definido no hay evidencia de permiso.
    /// </summary>
    private static bool RolVigente(RbacRol rol) => rol.EsActivo == true;

    /// <summary>
    /// El rol debe tener al menos un permiso activo que llegue hasta un modulo cuyo
    /// sistema sea 'titl'. Un rol llamado 'titul_algo' pero sin permisos en titulacion
    /// no concede acceso.
    /// </summary>
    private static bool ConcedePermisosEnTitulacion(RbacRol rol) =>
        rol.RbacRolModuloOperacions.Any(permiso =>
            permiso.EsActivo == true &&
            permiso.IdModulosOperacionesNavigation is { } moduloOperacion &&
            moduloOperacion.EsActivo == true &&
            moduloOperacion.IdModulosNavigation is { } modulo &&
            modulo.EsActivo == true &&
            modulo.IdSistemaNavigation is { } sistema &&
            string.Equals(
                sistema.Codigo?.Trim(),
                RbacTitulacion.Codigo,
                StringComparison.OrdinalIgnoreCase));
}
