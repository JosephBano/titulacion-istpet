using FluentAssertions;
using TitulacionIstpet.Domain.Entities;
using TitulacionIstpet.Domain.Enums;
using TitulacionIstpet.Domain.Exceptions;
using Xunit;

namespace TitulacionIstpet.Domain.Tests;

public class EstudianteTests
{
    private static Estudiante Nuevo() =>
        new("1712345678", " Ana ", " Perez ", "  ANA.PEREZ@ISTPET.EDU.EC ");

    [Fact]
    public void Constructor_normaliza_espacios_y_correo()
    {
        var e = Nuevo();

        e.Nombres.Should().Be("Ana");
        e.Apellidos.Should().Be("Perez");
        e.CorreoInstitucional.Should().Be("ana.perez@istpet.edu.ec");
        e.Estado.Should().Be(EstadoTitulacion.Borrador);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_rechaza_cedula_vacia(string cedula)
    {
        var accion = () => new Estudiante(cedula, "Ana", "Perez", "a@b.ec");

        accion.Should().Throw<DominioException>().WithMessage("*cedula*");
    }

    [Fact]
    public void Titulado_es_estado_terminal()
    {
        var e = Nuevo();
        e.AvanzarA(EstadoTitulacion.Titulado);

        var accion = () => e.AvanzarA(EstadoTitulacion.Borrador);

        accion.Should().Throw<DominioException>();
    }
}
