using FluentAssertions;
using NSubstitute;
using TitulacionIstpet.Application.Auth;
using TitulacionIstpet.Domain.Auth;
using TitulacionIstpet.Domain.Entities;
using Xunit;

namespace TitulacionIstpet.Application.Tests.Auth;

/// <summary>
/// Flujo de login completo. El eje de estos tests es que TODA falla —usuario
/// inexistente, cuenta inactiva, contrasenia mala, sin permisos sobre titulacion—
/// produzca exactamente el mismo error, para no filtrar que cuentas existen.
/// </summary>
public class AutenticarUsuarioTests
{
    private readonly IRepositorioAutenticacion _repositorio =
        Substitute.For<IRepositorioAutenticacion>();
    private readonly IVerificadorCredenciales _verificador =
        Substitute.For<IVerificadorCredenciales>();

    private const string IdSigafi = "1712345678";
    private const string Contrasenia = "clave-correcta";

    private AutenticarUsuario Sut() => new(_repositorio, _verificador);

    private static Usuario UsuarioActivo(bool activo = true) => new()
    {
        IdUsuario = 100,
        IdSigafi = IdSigafi,
        TablaSigafi = "alumno",
        Nombre = "Ana Perez",
        Contrasenia = "$2a$11$loquesea",
        Activo = activo
    };

    /// <summary>Deja el repositorio y el verificador en el camino feliz.</summary>
    private void DadoUnLoginValido(bool requiereRehash = false)
    {
        _repositorio.BuscarPorIdSigafiAsync(IdSigafi, Arg.Any<CancellationToken>())
            .Returns(UsuarioActivo());
        _verificador.Verificar(Contrasenia, Arg.Any<string>())
            .Returns(ResultadoVerificacion.Valida(requiereRehash));
        _repositorio.ObtenerAsignacionesAsync(100, Arg.Any<CancellationToken>())
            .Returns([AsignacionDeTitulacion("titul_secretario")]);
    }

    private static RbacUsuarioRol AsignacionDeTitulacion(string codigoRol)
    {
        var sistema = new RbacSistema { IdSistema = 1, Codigo = RbacTitulacion.Codigo, Detalle = "Titulacion" };
        var modulo = new RbacModulo { IdModulos = 1, EsActivo = true, IdSistemaNavigation = sistema };
        var moduloOperacion = new RbacModulosOperacione
        {
            IdModulosOperaciones = 1,
            EsActivo = true,
            IdModulosNavigation = modulo
        };
        var rol = new RbacRol { IdRol = 1, CodigoRol = codigoRol, Nombre = codigoRol, EsActivo = true };
        rol.RbacRolModuloOperacions.Add(new RbacRolModuloOperacion
        {
            EsActivo = true,
            IdRolNavigation = rol,
            IdModulosOperacionesNavigation = moduloOperacion
        });

        return new RbacUsuarioRol { IdUsuario = 100, IdRol = 1, EsActivo = true, IdRolNavigation = rol };
    }

    // ---------------------------------------------------------------------------

    [Fact]
    public async Task Autentica_y_devuelve_la_identidad_con_sus_roles()
    {
        DadoUnLoginValido();

        var identidad = await Sut().EjecutarAsync(IdSigafi, Contrasenia);

        identidad.IdUsuario.Should().Be(100);
        identidad.IdSigafi.Should().Be(IdSigafi);
        identidad.Nombre.Should().Be("Ana Perez");
        identidad.TieneRol("titul_secretario").Should().BeTrue();
    }

    [Fact]
    public async Task Recorta_los_espacios_del_identificador_antes_de_buscar()
    {
        DadoUnLoginValido();

        await Sut().EjecutarAsync("  " + IdSigafi + "  ", Contrasenia);

        await _repositorio.Received(1).BuscarPorIdSigafiAsync(IdSigafi, Arg.Any<CancellationToken>());
    }

    // ---------------------------------------------------------------------------
    //  TODAS LAS FALLAS SON INDISTINGUIBLES
    // ---------------------------------------------------------------------------

    [Fact]
    public async Task Rechaza_un_usuario_inexistente()
    {
        _repositorio.BuscarPorIdSigafiAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns((Usuario?)null);

        await Sut().Invoking(s => s.EjecutarAsync(IdSigafi, Contrasenia))
            .Should().ThrowAsync<CredencialesInvalidasException>()
            .WithMessage(CredencialesInvalidasException.MensajeGenerico);
    }

    [Fact]
    public async Task Rechaza_una_cuenta_inactiva_sin_llegar_a_verificar_la_contrasenia()
    {
        _repositorio.BuscarPorIdSigafiAsync(IdSigafi, Arg.Any<CancellationToken>())
            .Returns(UsuarioActivo(activo: false));

        await Sut().Invoking(s => s.EjecutarAsync(IdSigafi, Contrasenia))
            .Should().ThrowAsync<CredencialesInvalidasException>();

        _verificador.DidNotReceive().Verificar(Arg.Any<string>(), Arg.Any<string>());
    }

    [Fact]
    public async Task Rechaza_una_contrasenia_incorrecta_sin_consultar_los_roles()
    {
        _repositorio.BuscarPorIdSigafiAsync(IdSigafi, Arg.Any<CancellationToken>())
            .Returns(UsuarioActivo());
        _verificador.Verificar(Arg.Any<string>(), Arg.Any<string>())
            .Returns(ResultadoVerificacion.Fallida);

        await Sut().Invoking(s => s.EjecutarAsync(IdSigafi, "mala"))
            .Should().ThrowAsync<CredencialesInvalidasException>();

        await _repositorio.DidNotReceive()
            .ObtenerAsignacionesAsync(Arg.Any<int>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Rechaza_credenciales_validas_sin_permisos_sobre_titulacion()
    {
        // El caso que define el aislamiento: usuario real de otro sistema, contrasenia
        // correcta sobre la tabla compartida, pero sin roles de titulacion.
        _repositorio.BuscarPorIdSigafiAsync(IdSigafi, Arg.Any<CancellationToken>())
            .Returns(UsuarioActivo());
        _verificador.Verificar(Arg.Any<string>(), Arg.Any<string>())
            .Returns(ResultadoVerificacion.Valida());
        _repositorio.ObtenerAsignacionesAsync(100, Arg.Any<CancellationToken>())
            .Returns([]);

        await Sut().Invoking(s => s.EjecutarAsync(IdSigafi, Contrasenia))
            .Should().ThrowAsync<CredencialesInvalidasException>()
            .WithMessage(CredencialesInvalidasException.MensajeGenerico);
    }

    [Theory]
    [InlineData(null, "clave")]
    [InlineData("", "clave")]
    [InlineData("   ", "clave")]
    [InlineData("usuario", null)]
    [InlineData("usuario", "")]
    public async Task Rechaza_entradas_vacias_sin_tocar_la_base(string? usuario, string? clave)
    {
        await Sut().Invoking(s => s.EjecutarAsync(usuario, clave))
            .Should().ThrowAsync<CredencialesInvalidasException>();

        await _repositorio.DidNotReceive()
            .BuscarPorIdSigafiAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    // ---------------------------------------------------------------------------
    //  MIGRACION PROGRESIVA DE CONTRASENIAS
    // ---------------------------------------------------------------------------

    [Fact]
    public async Task Migra_a_bcrypt_cuando_la_contrasenia_estaba_en_texto_plano()
    {
        DadoUnLoginValido(requiereRehash: true);
        _verificador.Hashear(Contrasenia).Returns("$2a$11$hash-nuevo");

        await Sut().EjecutarAsync(IdSigafi, Contrasenia);

        await _repositorio.Received(1)
            .ActualizarContraseniaAsync(100, "$2a$11$hash-nuevo", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task No_regraba_nada_cuando_la_contrasenia_ya_estaba_hasheada()
    {
        DadoUnLoginValido(requiereRehash: false);

        await Sut().EjecutarAsync(IdSigafi, Contrasenia);

        await _repositorio.DidNotReceive().ActualizarContraseniaAsync(
            Arg.Any<int>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Un_fallo_al_migrar_no_impide_el_login()
    {
        // La migracion es oportunista. Si la escritura falla, el usuario ya se autentico
        // y no se le puede negar la entrada; se reintentara el proximo ingreso.
        DadoUnLoginValido(requiereRehash: true);
        _verificador.Hashear(Contrasenia).Returns("$2a$11$hash-nuevo");
        _repositorio.ActualizarContraseniaAsync(
                Arg.Any<int>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException(new InvalidOperationException("base caida")));

        var identidad = await Sut().EjecutarAsync(IdSigafi, Contrasenia);

        identidad.IdUsuario.Should().Be(100);
    }

    [Fact]
    public async Task No_migra_la_contrasenia_de_un_usuario_al_que_se_le_niega_el_acceso()
    {
        // Sin roles de titulacion no hay login, y por tanto tampoco escritura en la
        // tabla compartida: un rechazo no debe dejar rastro.
        _repositorio.BuscarPorIdSigafiAsync(IdSigafi, Arg.Any<CancellationToken>())
            .Returns(UsuarioActivo());
        _verificador.Verificar(Arg.Any<string>(), Arg.Any<string>())
            .Returns(ResultadoVerificacion.Valida(requiereRehash: true));
        _repositorio.ObtenerAsignacionesAsync(100, Arg.Any<CancellationToken>()).Returns([]);

        await Sut().Invoking(s => s.EjecutarAsync(IdSigafi, Contrasenia))
            .Should().ThrowAsync<CredencialesInvalidasException>();

        await _repositorio.DidNotReceive().ActualizarContraseniaAsync(
            Arg.Any<int>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }
}
