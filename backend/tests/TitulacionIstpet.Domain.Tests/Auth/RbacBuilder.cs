using TitulacionIstpet.Domain.Auth;
using TitulacionIstpet.Domain.Entities;

namespace TitulacionIstpet.Domain.Tests.Auth;

/// <summary>
/// Arma el grafo RBAC en memoria. Sin esto cada test necesitaria seis entidades
/// encadenadas a mano y la intencion quedaria sepultada bajo el andamiaje.
///
///   RbacUsuarioRol -> RbacRol -> RbacRolModuloOperacion -> RbacModulosOperacione
///                            -> RbacModulo -> RbacSistema
/// </summary>
internal sealed class RbacBuilder
{
    private string _codigoRol = "titul_secretario";
    private string _codigoSistema = RbacTitulacion.Codigo;
    private bool? _asignacionActiva = true;
    private bool? _rolActivo = true;
    private bool? _permisoActivo = true;
    private bool? _moduloOperacionActivo = true;
    private bool? _moduloActivo = true;
    private bool _conPermisos = true;

    public static RbacBuilder Rol(string codigoRol) => new() { _codigoRol = codigoRol };

    public RbacBuilder EnSistema(string codigo) { _codigoSistema = codigo; return this; }
    public RbacBuilder ConAsignacionActiva(bool? v) { _asignacionActiva = v; return this; }
    public RbacBuilder ConRolActivo(bool? v) { _rolActivo = v; return this; }
    public RbacBuilder ConPermisoActivo(bool? v) { _permisoActivo = v; return this; }
    public RbacBuilder ConModuloOperacionActivo(bool? v) { _moduloOperacionActivo = v; return this; }
    public RbacBuilder ConModuloActivo(bool? v) { _moduloActivo = v; return this; }
    public RbacBuilder SinNingunPermiso() { _conPermisos = false; return this; }

    public RbacUsuarioRol Construir()
    {
        var rol = new RbacRol
        {
            IdRol = 1,
            Nombre = _codigoRol,
            CodigoRol = _codigoRol,
            EsActivo = _rolActivo
        };

        if (_conPermisos)
        {
            var sistema = new RbacSistema
            {
                IdSistema = 1,
                Codigo = _codigoSistema,
                Detalle = $"Sistema {_codigoSistema}"
            };

            var modulo = new RbacModulo
            {
                IdModulos = 1,
                IdSistema = sistema.IdSistema,
                Nombre = "Modulo",
                EsActivo = _moduloActivo,
                IdSistemaNavigation = sistema
            };

            var moduloOperacion = new RbacModulosOperacione
            {
                IdModulosOperaciones = 1,
                IdModulos = modulo.IdModulos,
                IdOperaciones = 1,
                EsActivo = _moduloOperacionActivo,
                IdModulosNavigation = modulo,
                IdOperacionesNavigation = new RbacOperacione
                {
                    IdOperaciones = 1,
                    NombreOperacion = "consultar"
                }
            };

            rol.RbacRolModuloOperacions.Add(new RbacRolModuloOperacion
            {
                IdRolModuloOperacion = 1,
                IdRol = rol.IdRol,
                IdModulosOperaciones = moduloOperacion.IdModulosOperaciones,
                EsActivo = _permisoActivo,
                IdRolNavigation = rol,
                IdModulosOperacionesNavigation = moduloOperacion
            });
        }

        return new RbacUsuarioRol
        {
            IdUsuarioRol = 1,
            IdUsuario = 100,
            IdRol = rol.IdRol,
            EsActivo = _asignacionActiva,
            IdRolNavigation = rol
        };
    }
}
