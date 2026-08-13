using FluentAssertions;
using Titan.Domain.Entities;
using Xunit;

namespace Titan.Domain.Tests;

public class AlumnoTests
{
    [Fact]
    public void Instanciacion_alumno_asigna_propiedades_correctamente()
    {
        var alumno = new alumnos
        {
            idAlumno = "0602959553",
            primerNombre = "Jorge",
            apellidoPaterno = "Doicela",
            email = "jorge.doicela@istpet.edu.ec"
        };

        alumno.idAlumno.Should().Be("0602959553");
        alumno.primerNombre.Should().Be("Jorge");
        alumno.apellidoPaterno.Should().Be("Doicela");
        alumno.email.Should().Be("jorge.doicela@istpet.edu.ec");
    }
}
