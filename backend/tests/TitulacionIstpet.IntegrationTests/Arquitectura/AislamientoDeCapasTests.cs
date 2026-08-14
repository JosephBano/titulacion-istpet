using System.Reflection;
using FluentAssertions;
using NetArchTest.Rules;
using TitulacionIstpet.Application.Auth;
using TitulacionIstpet.Domain.Auth;
using TitulacionIstpet.Infrastructure.Auth;
using Xunit;

namespace TitulacionIstpet.IntegrationTests.Arquitectura;

/// <summary>
/// Reglas de aislamiento de capas. Rompen el build en cuanto alguien cruce una frontera.
///
/// Viven en el proyecto de integracion, no en el de aplicacion, por una razon concreta:
/// NetArchTest solo ve tipos de ensamblados cargados, y Application.Tests unicamente
/// referencia Application. Desde ahi, las reglas sobre WebApi e Infrastructure no
/// encontrarian ningun tipo y pasarian en vacio: verde sin haber verificado nada.
/// IntegrationTests referencia WebApi, que arrastra las cuatro capas.
/// </summary>
public class AislamientoDeCapasTests
{
    private const string Controladores = "TitulacionIstpet.WebApi.Controllers";
    private const string Application = "TitulacionIstpet.Application";
    private const string Domain = "TitulacionIstpet.Domain";
    private const string DomainEntities = "TitulacionIstpet.Domain.Entities";
    private const string Infrastructure = "TitulacionIstpet.Infrastructure";
    private const string EfCore = "Microsoft.EntityFrameworkCore";

    // Un tipo ancla por capa fuerza la carga del ensamblado correspondiente.
    private static readonly Assembly[] Capas =
    [
        typeof(Program).Assembly,                            // WebApi
        typeof(AutenticarUsuario).Assembly,                  // Application
        typeof(RbacTitulacion).Assembly,                  // Domain
        typeof(VerificadorCredencialesBcrypt).Assembly       // Infrastructure
    ];

    /// <summary>
    /// Centinela del propio mecanismo. Si el ensamblado de WebApi dejara de cargarse,
    /// las reglas sobre Controllers pasarian en verde sin evaluar nada. Este test
    /// falla primero y deja claro que el problema es el escaneo, no el codigo.
    /// </summary>
    [Fact]
    public void El_escaneo_alcanza_las_cuatro_capas()
    {
        foreach (var capa in Capas)
        {
            Types.InAssemblies([capa]).GetTypes().Should().NotBeEmpty(
                $"el ensamblado {capa.GetName().Name} debe ser visible para NetArchTest");
        }
    }

    // Las dos reglas siguientes no exigen que existan tipos: todavia no hay controllers
    // y ese estado es legitimo. No pueden pasar en falso por un fallo de carga porque
    // El_escaneo_alcanza_las_cuatro_capas lo cubre; se activan solas con el primer controller.

    [Fact]
    public void Controllers_NoDebenDependerDe_EfCore() =>
        Afirmar(Controladores, EfCore, "Controllers no deben usar EF Core directamente",
            exigirTipos: false);

    [Fact]
    public void Controllers_NoDebenDependerDe_EntidadesDeDominio() =>
        Afirmar(Controladores, DomainEntities,
            "Controllers exponen DTOs de Application, no entidades de Domain",
            exigirTipos: false);

    [Fact]
    public void Application_NoDebeDependerDe_EfCore() =>
        Afirmar(Application, EfCore,
            "Application solo conoce sus puertos; EF vive en Infrastructure");

    [Fact]
    public void Domain_NoDebeDependerDe_Infrastructure() =>
        Afirmar(Domain, Infrastructure,
            "Domain es la capa mas interna; no puede conocer Infrastructure");

    [Fact]
    public void Domain_NoDebeDependerDe_EfCore() =>
        Afirmar(Domain, EfCore,
            "Domain no debe arrastrar EF Core ni como dependencia transitiva");

    /// <summary>
    /// Aplica la regla.
    ///
    /// <paramref name="exigirTipos"/> vale true en las capas que siempre estan pobladas:
    /// ahi, encontrar cero tipos significa que el escaneo se rompio, y pasar en verde
    /// seria afirmar que la capa cumple sin haberla mirado.
    /// </summary>
    private static void Afirmar(
        string espacio, string prohibido, string mensaje, bool exigirTipos = true)
    {
        var alcance = Types.InAssemblies(Capas).That().ResideInNamespaceStartingWith(espacio);

        if (exigirTipos)
        {
            alcance.GetTypes().Should().NotBeEmpty(
                $"la regla '{mensaje}' necesita tipos en '{espacio}' para significar algo");
        }

        var resultado = alcance.ShouldNot().HaveDependencyOn(prohibido).GetResult();

        if (resultado.IsSuccessful)
        {
            return;
        }

        string ofensores = string.Join(
            Environment.NewLine,
            resultado.FailingTypeNames.Select(n => "  - " + n));

        Assert.Fail($"{mensaje}. Tipos que rompen la regla:{Environment.NewLine}{ofensores}");
    }
}
