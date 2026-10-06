using FluentAssertions;
using NSubstitute;
using TitulacionIstpet.Application.Common.Models;
using TitulacionIstpet.Application.Features.Complexivo;

namespace TitulacionIstpet.Application.Tests.Features.Complexivo;

public class AdministrarComplexivoTests
{
    private readonly IRepositorioComplexivo _repo = Substitute.For<IRepositorioComplexivo>();

    private static GuardarMallaComplexivoDto Valido() => new(
        1, "Módulo 1", "Fundamentos", new DateOnly(2026, 10, 5), new DateOnly(2026, 10, 24),
        "Mixta", 32, true, [new TemaComplexivoDto(0, 10, "Contenidos", 32)],
        [new DocenteComplexivoDto(0, null, null, null, true, [new HorarioComplexivoDto(0, "Lunes a jueves", "19:00 a 21:00 (virtual)")])]);

    [Fact]
    public async Task Configuracion_valida_permite_docente_pendiente_y_guarda_agregado()
    {
        var caso = new AdministrarComplexivo(_repo, new GuardarMallaComplexivoValidator());
        var dto = Valido();
        _repo.GuardarAsync(null, dto, Arg.Any<CancellationToken>()).Returns(42);

        (await caso.GuardarAsync(null, dto, CancellationToken.None)).Should().Be(42);
        await _repo.Received(1).GuardarAsync(null, dto, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Horas_inconsistentes_no_llegan_a_persistencia()
    {
        var caso = new AdministrarComplexivo(_repo, new GuardarMallaComplexivoValidator());
        var accion = () => caso.GuardarAsync(null, Valido() with { TotalHoras = 31 }, CancellationToken.None);

        await accion.Should().ThrowAsync<ValidacionException>();
        await _repo.DidNotReceiveWithAnyArgs().GuardarAsync(default, default!, default);
    }

    [Fact]
    public void Fechas_invertidas_y_textos_fuera_del_limite_son_rechazados()
    {
        var dto = Valido() with { FechaFin = new DateOnly(2026, 10, 1), Detalle = new string('a', 151) };
        var resultado = new GuardarMallaComplexivoValidator().Validate(dto);

        resultado.Errors.Should().Contain(e => e.PropertyName == "FechaFin");
        resultado.Errors.Should().Contain(e => e.PropertyName == "Detalle");
    }

    [Fact]
    public void Colecciones_nulas_o_elementos_nulos_devuelven_errores_de_validacion()
    {
        var validator = new GuardarMallaComplexivoValidator();
        validator.Validate(Valido() with { Temas = null!, Docentes = null! }).IsValid.Should().BeFalse();
        validator.Validate(Valido() with { Temas = [null!] }).IsValid.Should().BeFalse();
        validator.Validate(Valido() with { Docentes = [Valido().Docentes[0] with { Horarios = [null!] }] }).IsValid.Should().BeFalse();
    }

    [Fact]
    public void Temas_existentes_repetidos_son_rechazados()
    {
        var tema = new TemaComplexivoDto(12, 10, "Contenido", 16);
        new GuardarMallaComplexivoValidator().Validate(Valido() with { Temas = [tema, tema] }).IsValid.Should().BeFalse();
    }
}
