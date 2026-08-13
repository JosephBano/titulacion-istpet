using FluentAssertions;
using TitulacionIstpet.Infrastructure.Auth;
using Xunit;

namespace TitulacionIstpet.Application.Tests.Auth;

/// <summary>
/// La tabla `usuarios` es compartida con los demas sistemas del ISTPET y contiene
/// contrasenias en BCrypt y en texto plano a la vez. Estos tests fijan que ambos
/// formatos sigan funcionando: romper el legacy dejaria a esos usuarios fuera de
/// titulacion pero dentro del resto de sistemas.
/// </summary>
public class VerificadorCredencialesBcryptTests
{
    private readonly VerificadorCredencialesBcrypt _verificador = new();

    private const string Contrasenia = "S3cr3t0-ISTPET";

    // Hash real generado con work factor 11, el mismo que usa auth_global.
    private static string HashDe(string valor) =>
        BCrypt.Net.BCrypt.HashPassword(valor, VerificadorCredencialesBcrypt.WorkFactor);

    [Fact]
    public void WorkFactor_coincide_con_auth_global()
    {
        // Si auth_global cambia su PasswordService.WorkFactor, este test es el recordatorio
        // de sincronizar. Un desajuste no rompe la verificacion (BCrypt guarda el coste en
        // el hash) pero deja el ecosistema con costos heterogeneos.
        VerificadorCredencialesBcrypt.WorkFactor.Should().Be(11);
    }

    [Fact]
    public void Acepta_una_contrasenia_hasheada_correcta_sin_pedir_rehash()
    {
        var resultado = _verificador.Verificar(Contrasenia, HashDe(Contrasenia));

        resultado.EsValida.Should().BeTrue();
        resultado.RequiereRehash.Should().BeFalse();
    }

    [Fact]
    public void Rechaza_una_contrasenia_hasheada_incorrecta()
    {
        var resultado = _verificador.Verificar("otra-cosa", HashDe(Contrasenia));

        resultado.EsValida.Should().BeFalse();
    }

    [Fact]
    public void Acepta_texto_plano_legacy_y_pide_rehash()
    {
        var resultado = _verificador.Verificar(Contrasenia, Contrasenia);

        resultado.EsValida.Should().BeTrue();
        resultado.RequiereRehash.Should().BeTrue("una coincidencia en claro debe migrar a BCrypt");
    }

    [Fact]
    public void Acepta_texto_plano_con_relleno_de_espacios()
    {
        // Las columnas CHAR del esquema legacy vienen con padding. auth_global hace Trim()
        // y aqui se replica: sin esto, esos usuarios no podrian entrar.
        var resultado = _verificador.Verificar(Contrasenia, "  " + Contrasenia + "   ");

        resultado.EsValida.Should().BeTrue();
        resultado.RequiereRehash.Should().BeTrue();
    }

    [Fact]
    public void No_recorta_la_contrasenia_que_escribe_el_usuario()
    {
        // Solo se recorta lo almacenado, nunca la entrada: si se recortara la entrada,
        // "  clave  " abriria la cuenta de "clave".
        var resultado = _verificador.Verificar("  " + Contrasenia + "  ", Contrasenia);

        resultado.EsValida.Should().BeFalse();
    }

    [Theory]
    [InlineData(null, "algo")]
    [InlineData("", "algo")]
    [InlineData("algo", null)]
    [InlineData("algo", "")]
    [InlineData(null, null)]
    public void Rechaza_entradas_vacias_o_nulas(string? enClaro, string? almacenado)
    {
        _verificador.Verificar(enClaro, almacenado).EsValida.Should().BeFalse();
    }

    [Fact]
    public void Un_hash_corrupto_falla_como_credencial_invalida_y_no_como_excepcion()
    {
        // Truncar un hash en la base es un accidente real (columnas mal dimensionadas al
        // migrar). Debe rechazar el login, no tumbar el endpoint con una excepcion.
        var accion = () => _verificador.Verificar(Contrasenia, "$2a$11$truncado");

        accion.Should().NotThrow();
        _verificador.Verificar(Contrasenia, "$2a$11$truncado").EsValida.Should().BeFalse();
    }

    [Theory]
    [InlineData("$2a$11$abcdefghijklmnopqrstuv", true)]
    [InlineData("$2y$11$abcdefghijklmnopqrstuv", true)]
    [InlineData("$2b$11$abcdefghijklmnopqrstuv", true)]
    [InlineData("textoplano", false)]
    [InlineData("$1$md5cosa", false)]
    [InlineData("$2", false)]
    [InlineData("", false)]
    [InlineData(null, false)]
    public void EsHash_detecta_el_prefijo_bcrypt(string? valor, bool esperado)
    {
        // Mismo criterio que PasswordService.IsHashed de auth_global: prefijo '$2' y
        // longitud minima 4. Las variantes 2a/2y/2b conviven en la base segun que
        // libreria genero el hash.
        _verificador.EsHash(valor).Should().Be(esperado);
    }

    [Fact]
    public void Hashear_produce_un_valor_verificable_y_distinto_cada_vez()
    {
        string primero = _verificador.Hashear(Contrasenia);
        string segundo = _verificador.Hashear(Contrasenia);

        primero.Should().NotBe(segundo, "cada hash lleva su propia sal");
        _verificador.Verificar(Contrasenia, primero).EsValida.Should().BeTrue();
        _verificador.Verificar(Contrasenia, segundo).EsValida.Should().BeTrue();
    }

    [Fact]
    public void Un_hash_generado_aqui_es_verificable_con_la_libreria_de_auth_global()
    {
        // Contrato de interoperabilidad: lo que titulacion escribe en la tabla compartida
        // tiene que poder leerlo cualquier otro sistema del ecosistema.
        string hash = _verificador.Hashear(Contrasenia);

        BCrypt.Net.BCrypt.Verify(Contrasenia, hash).Should().BeTrue();
    }

    [Fact]
    public void Una_contrasenia_hasheada_no_se_valida_comparandola_como_texto_plano()
    {
        // Si alguien pasa el hash como si fuera la contrasenia, no debe entrar.
        string hash = HashDe(Contrasenia);

        _verificador.Verificar(hash, hash).EsValida.Should().BeFalse();
    }
}
