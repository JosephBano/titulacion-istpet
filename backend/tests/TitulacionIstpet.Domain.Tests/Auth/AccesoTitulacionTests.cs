using FluentAssertions;
using TitulacionIstpet.Domain.Auth;
using TitulacionIstpet.Domain.Entities;
using Xunit;

namespace TitulacionIstpet.Domain.Tests.Auth;

/// <summary>
/// El aislamiento entre sistemas es la propiedad de seguridad central de este modulo:
/// la base RBAC es compartida por todo el ISTPET, y un error aqui deja entrar a
/// titulacion a usuarios de gestion academica, bienestar o RRHH.
/// </summary>
public class AccesoTitulacionTests
{
    [Fact]
    public void Concede_acceso_a_un_rol_de_titulacion_bien_formado()
    {
        var asignaciones = new[] { RbacBuilder.Rol("titul_secretario").Construir() };

        var roles = AccesoTitulacion.ResolverRoles(asignaciones);

        roles.Should().ContainSingle().Which.Should().Be("titul_secretario");
        AccesoTitulacion.TieneAcceso(asignaciones).Should().BeTrue();
    }

    // ---------------------------------------------------------------------------
    //  FAIL-CLOSED
    // ---------------------------------------------------------------------------

    [Fact]
    public void Un_usuario_sin_ninguna_asignacion_no_tiene_acceso()
    {
        // auth_global, ante cero permisos, devuelve TODOS los sistemas
        // (AuthService.GetUsuarioSistemasAsync). Aqui la ausencia de permisos niega.
        var roles = AccesoTitulacion.ResolverRoles([]);

        roles.Should().BeEmpty();
        AccesoTitulacion.TieneAcceso([]).Should().BeFalse();
    }

    [Fact]
    public void Un_rol_de_titulacion_sin_ningun_permiso_no_concede_acceso()
    {
        // El rol se llama bien pero no esta vinculado a ningun modulo: no hay evidencia
        // de que deba entrar. Fail-closed.
        var asignaciones = new[] { RbacBuilder.Rol("titul_fantasma").SinNingunPermiso().Construir() };

        AccesoTitulacion.ResolverRoles(asignaciones).Should().BeEmpty();
    }

    // ---------------------------------------------------------------------------
    //  AISLAMIENTO ENTRE SISTEMAS
    // ---------------------------------------------------------------------------

    [Theory]
    [InlineData("acad")]
    [InlineData("bien")]
    [InlineData("rrhh")]
    [InlineData("admin")]
    public void Un_rol_de_otro_sistema_no_concede_acceso(string otroSistema)
    {
        var asignaciones = new[]
        {
            RbacBuilder.Rol("acad_docente").EnSistema(otroSistema).Construir()
        };

        AccesoTitulacion.ResolverRoles(asignaciones).Should().BeEmpty();
    }

    [Fact]
    public void Un_rol_con_prefijo_correcto_pero_permisos_en_otro_sistema_no_concede_acceso()
    {
        // Esta es la razon de que el prefijo no alcance: alguien crea 'titul_x' pero lo
        // cuelga de modulos de bienestar. El nombre miente; el grafo manda.
        var asignaciones = new[]
        {
            RbacBuilder.Rol("titul_impostor").EnSistema("bien").Construir()
        };

        AccesoTitulacion.ResolverRoles(asignaciones).Should().BeEmpty();
    }

    [Fact]
    public void Un_rol_con_permisos_en_titulacion_pero_sin_el_prefijo_no_concede_acceso()
    {
        // Y esta es la razon de que el grafo tampoco alcance solo: un rol de otro sistema
        // al que por error le colgaron un modulo de titulacion no debe entrar.
        var asignaciones = new[]
        {
            RbacBuilder.Rol("acad_coordinador").EnSistema(RbacTitulacion.Codigo).Construir()
        };

        AccesoTitulacion.ResolverRoles(asignaciones).Should().BeEmpty();
    }

    [Fact]
    public void De_varios_roles_mezclados_solo_sobreviven_los_de_titulacion()
    {
        var asignaciones = new[]
        {
            RbacBuilder.Rol("acad_docente").EnSistema("acad").Construir(),
            RbacBuilder.Rol("titul_secretario").Construir(),
            RbacBuilder.Rol("bien_trabajador").EnSistema("bien").Construir(),
            RbacBuilder.Rol("titul_coordinador").Construir()
        };

        var roles = AccesoTitulacion.ResolverRoles(asignaciones);

        roles.Should().BeEquivalentTo(["titul_secretario", "titul_coordinador"]);
    }

    [Fact]
    public void El_codigo_del_sistema_se_compara_ignorando_espacios_y_caja()
    {
        // Las columnas CHAR del esquema legacy traen padding y las colaciones _ci de
        // MySQL no distinguen mayusculas: el codigo debe tolerar ambas cosas.
        var asignaciones = new[] { RbacBuilder.Rol("titul_x").EnSistema("  TITL ").Construir() };

        AccesoTitulacion.ResolverRoles(asignaciones).Should().ContainSingle();
    }

    // ---------------------------------------------------------------------------
    //  SEMANTICA DE EsActivo  (bool? en toda la cadena)
    // ---------------------------------------------------------------------------

    [Theory]
    [InlineData(false)]
    [InlineData(null)]
    public void Una_asignacion_no_activa_no_concede_acceso(bool? estado)
    {
        // En la asignacion usuario-rol, NULL se trata como inactivo: una fila sin estado
        // definido no es evidencia de permiso.
        var asignaciones = new[]
        {
            RbacBuilder.Rol("titul_secretario").ConAsignacionActiva(estado).Construir()
        };

        AccesoTitulacion.ResolverRoles(asignaciones).Should().BeEmpty();
    }

    [Theory]
    [InlineData(false)]
    [InlineData(null)]
    public void Un_rol_no_activo_no_concede_acceso(bool? estado)
    {
        // NULL es inactivo tambien aqui. Diverge de auth_global, que acepta
        // "EsActivo == 1 || null": un usuario cuyo unico rol tenga la columna en NULL
        // entrara a los otros sistemas pero no a titulacion.
        var asignaciones = new[]
        {
            RbacBuilder.Rol("titul_secretario").ConRolActivo(estado).Construir()
        };

        AccesoTitulacion.ResolverRoles(asignaciones).Should().BeEmpty();
    }

    [Fact]
    public void Solo_un_EsActivo_explicitamente_verdadero_concede_acceso_en_toda_la_cadena()
    {
        // Fija la regla de una vez para los cinco eslabones: cualquiera en NULL o en
        // false corta el acceso. Es el contrato que resume a los tests de arriba.
        var completo = RbacBuilder.Rol("titul_secretario").Construir();

        AccesoTitulacion.ResolverRoles([completo]).Should().ContainSingle();
    }

    [Theory]
    [InlineData(false)]
    [InlineData(null)]
    public void Un_permiso_no_activo_no_concede_acceso(bool? estado)
    {
        var asignaciones = new[]
        {
            RbacBuilder.Rol("titul_secretario").ConPermisoActivo(estado).Construir()
        };

        AccesoTitulacion.ResolverRoles(asignaciones).Should().BeEmpty();
    }

    [Theory]
    [InlineData(false)]
    [InlineData(null)]
    public void Un_modulo_operacion_no_activo_no_concede_acceso(bool? estado)
    {
        var asignaciones = new[]
        {
            RbacBuilder.Rol("titul_secretario").ConModuloOperacionActivo(estado).Construir()
        };

        AccesoTitulacion.ResolverRoles(asignaciones).Should().BeEmpty();
    }

    [Theory]
    [InlineData(false)]
    [InlineData(null)]
    public void Un_modulo_no_activo_no_concede_acceso(bool? estado)
    {
        // Desactivar el modulo es la palanca para cerrar titulacion entera sin tocar roles.
        var asignaciones = new[]
        {
            RbacBuilder.Rol("titul_secretario").ConModuloActivo(estado).Construir()
        };

        AccesoTitulacion.ResolverRoles(asignaciones).Should().BeEmpty();
    }

    // ---------------------------------------------------------------------------
    //  ROBUSTEZ ANTE DATOS INCOMPLETOS
    // ---------------------------------------------------------------------------

    [Fact]
    public void Una_asignacion_sin_rol_cargado_se_ignora_sin_reventar()
    {
        // Si alguien olvida un Include(), la navegacion llega nula. Debe negar el acceso,
        // no lanzar NullReferenceException dentro del flujo de login.
        var huerfana = new RbacUsuarioRol { IdUsuarioRol = 1, IdUsuario = 100, EsActivo = true };

        var accion = () => AccesoTitulacion.ResolverRoles([huerfana]);

        accion.Should().NotThrow();
        AccesoTitulacion.ResolverRoles([huerfana]).Should().BeEmpty();
    }

    [Fact]
    public void Roles_duplicados_se_reportan_una_sola_vez()
    {
        var asignaciones = new[]
        {
            RbacBuilder.Rol("titul_secretario").Construir(),
            RbacBuilder.Rol("titul_secretario").Construir()
        };

        AccesoTitulacion.ResolverRoles(asignaciones).Should().ContainSingle();
    }

    [Fact]
    public void Resolver_con_null_lanza_ArgumentNullException()
    {
        var accion = () => AccesoTitulacion.ResolverRoles(null!);

        accion.Should().Throw<ArgumentNullException>();
    }
}
