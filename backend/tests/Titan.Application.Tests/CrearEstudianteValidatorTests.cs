using FluentAssertions;
using Titan.Application.Features.Estudiantes.Commands;
using Xunit;

namespace Titan.Application.Tests;

public class CrearEstudianteValidatorTests
{
    private readonly CrearEstudianteValidator _validador = new();

    [Theory]
    [InlineData("171234567")]    // 9 digitos
    [InlineData("17123456789")]  // 11 digitos
    [InlineData("17123abcde")]
    public void Rechaza_cedulas_mal_formadas(string cedula)
    {
        var r = _validador.Validate(new CrearEstudianteCommand(cedula, "Ana", "Perez", "a@b.ec"));

        r.IsValid.Should().BeFalse();
        r.Errors.Should().Contain(e => e.PropertyName == nameof(CrearEstudianteCommand.Cedula));
    }

    [Fact]
    public void Acepta_un_comando_bien_formado()
    {
        var r = _validador.Validate(
            new CrearEstudianteCommand("1712345678", "Ana", "Perez", "ana@istpet.edu.ec"));

        r.IsValid.Should().BeTrue();
    }
}
