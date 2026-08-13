using FluentAssertions;
using Titan.Domain.Entities;
using Xunit;

namespace Titan.Application.Tests;

public class AuthTests
{
    [Fact]
    public void Usuario_valida_campos_de_credenciales()
    {
        var usuario = new usuarios
        {
            idUsuario = 1,
            idSigafi = "0602959553",
            activo = 1
        };

        usuario.idUsuario.Should().Be(1);
        usuario.idSigafi.Should().Be("0602959553");
        usuario.activo.Should().Be(1);
    }
}
