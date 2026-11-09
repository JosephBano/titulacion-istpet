using FluentAssertions;
using TitulacionIstpet.Domain.Entities;
using TitulacionIstpet.Domain.Exceptions;
using Xunit;

namespace TitulacionIstpet.Domain.Tests.Pagos;

public class TitulConfiguracionPagoModalidadTests
{
    [Fact]
    public void Examen_complexivo_con_700_base_y_cero_inversion_es_valido()
    {
        var config = new TitulConfiguracionPagoModalidad
        {
            IdModalidadTitulacion = 1,
            ValorBase = 700.00m,
            MaximoInversion = 0.00m,
            TotalTitulacion = 700.00m
        };

        var accion = () => config.ValidarCoherencia();

        accion.Should().NotThrow();
    }

    [Fact]
    public void Propuesta_tecnologica_con_500_base_y_200_inversion_es_valida()
    {
        var config = new TitulConfiguracionPagoModalidad
        {
            IdModalidadTitulacion = 2,
            ValorBase = 500.00m,
            MaximoInversion = 200.00m,
            TotalTitulacion = 700.00m
        };

        var accion = () => config.ValidarCoherencia();

        accion.Should().NotThrow();
    }

    [Fact]
    public void Inversion_superior_a_200_es_rechazada()
    {
        var config = new TitulConfiguracionPagoModalidad
        {
            IdModalidadTitulacion = 2,
            ValorBase = 499.00m,
            MaximoInversion = 201.00m,
            TotalTitulacion = 700.00m
        };

        var accion = () => config.ValidarCoherencia();

        accion.Should().Throw<DominioException>()
            .WithMessage("*no puede superar $200.00*");
    }

    [Fact]
    public void Suma_distinta_de_700_es_rechazada()
    {
        var config = new TitulConfiguracionPagoModalidad
        {
            IdModalidadTitulacion = 1,
            ValorBase = 650.00m,
            MaximoInversion = 0.00m,
            TotalTitulacion = 700.00m
        };

        var accion = () => config.ValidarCoherencia();

        accion.Should().Throw<DominioException>()
            .WithMessage("*debe coincidir con el total de titulación*");
    }

    [Theory]
    [InlineData(-10, 0)]
    [InlineData(700, -10)]
    public void Valores_negativos_son_rechazados(decimal valorBase, decimal maxInversion)
    {
        var config = new TitulConfiguracionPagoModalidad
        {
            IdModalidadTitulacion = 1,
            ValorBase = valorBase,
            MaximoInversion = maxInversion,
            TotalTitulacion = 700.00m
        };

        var accion = () => config.ValidarCoherencia();

        accion.Should().Throw<DominioException>()
            .WithMessage("*no puede ser negativo*");
    }
}
