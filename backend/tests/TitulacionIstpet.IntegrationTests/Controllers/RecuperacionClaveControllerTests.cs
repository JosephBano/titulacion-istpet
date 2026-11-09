using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using TitulacionIstpet.Application.DTOs.Auth;
using TitulacionIstpet.Application.Interfaces;
using TitulacionIstpet.WebApi.Controllers;
using Xunit;

namespace TitulacionIstpet.IntegrationTests.Controllers;

public class RecuperacionClaveControllerTests
{
    private readonly FakeAuthService _authService = new();
    private readonly AuthController _controller;

    public RecuperacionClaveControllerTests()
    {
        _controller = new AuthController(_authService);
    }

    [Fact]
    public async Task ValidarDocumento_RetornaOkConResultado()
    {
        var request = new ValidarDocumentoRecuperacionRequestDto { NumeroDocumento = "1712345678" };

        var result = await _controller.ValidarDocumentoRecuperacion(request, CancellationToken.None);

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<ValidarDocumentoRecuperacionResponseDto>().Subject;
        response.Valido.Should().BeTrue();
        response.NombreCompleto.Should().Be("Juan Perez");
    }

    [Fact]
    public async Task ValidarDocumento_CuandoNoEsApto_RetornaValidoFalse()
    {
        var request = new ValidarDocumentoRecuperacionRequestDto { NumeroDocumento = "1799999999" };

        var result = await _controller.ValidarDocumentoRecuperacion(request, CancellationToken.None);

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<ValidarDocumentoRecuperacionResponseDto>().Subject;
        response.Valido.Should().BeFalse();
        response.Mensaje.Should().Contain("exclusivamente para estudiantes del último semestre o egresados");
    }

    [Fact]
    public async Task ValidarFechaNacimiento_RetornaOkConResultado()
    {
        var request = new ValidarFechaNacimientoRequestDto
        {
            NumeroDocumento = "1712345678",
            FechaNacimiento = new DateOnly(1995, 5, 20)
        };

        var result = await _controller.ValidarFechaNacimiento(request, CancellationToken.None);

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<ValidarFechaNacimientoResponseDto>().Subject;
        response.Valido.Should().BeTrue();
    }

    [Fact]
    public async Task RestablecerContrasena_ConDatosValidos_RetornaOk()
    {
        var request = new RestablecerContrasenaRequestDto
        {
            NumeroDocumento = "1712345678",
            Token = "token-valido-prueba",
            NuevaContrasenia = "NuevaClave123*",
            ConfirmarContrasenia = "NuevaClave123*"
        };

        var result = await _controller.RestablecerContrasena(request, CancellationToken.None);

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<RestablecerContrasenaResponseDto>().Subject;
        response.Exito.Should().BeTrue();
        response.Mensaje.Should().Contain("exitosa");
    }

    [Fact]
    public async Task RestablecerContrasena_ConTokenInvalido_RetornaBadRequest()
    {
        var request = new RestablecerContrasenaRequestDto
        {
            NumeroDocumento = "1712345678",
            Token = "token-invalido",
            NuevaContrasenia = "NuevaClave123*",
            ConfirmarContrasenia = "NuevaClave123*"
        };

        var result = await _controller.RestablecerContrasena(request, CancellationToken.None);

        var badRequestResult = result.Should().BeOfType<BadRequestObjectResult>().Subject;
        var response = badRequestResult.Value.Should().BeOfType<RestablecerContrasenaResponseDto>().Subject;
        response.Exito.Should().BeFalse();
    }

    private sealed class FakeAuthService : IAuthService
    {
        public Task<LoginResponseDto> LoginAsync(LoginRequestDto request, string ipAddress, string deviceInfo, CancellationToken cancellationToken = default)
            => throw new NotImplementedException();

        public Task<LoginResponseDto> RefreshTokenAsync(RefreshTokenRequestDto request, string ipAddress, string deviceInfo, CancellationToken cancellationToken = default)
            => throw new NotImplementedException();

        public Task<bool> RevokeRefreshTokenAsync(string refreshToken, string reason, CancellationToken cancellationToken = default)
            => throw new NotImplementedException();

        public Task<UserPermissionsDto> GetUserPermissionsAsync(int idUsuario, string systemCode = "TITULACION", CancellationToken cancellationToken = default)
            => throw new NotImplementedException();

        public Task<ValidarDocumentoRecuperacionResponseDto> ValidarDocumentoRecuperacionAsync(ValidarDocumentoRecuperacionRequestDto request, CancellationToken cancellationToken = default)
        {
            if (request.NumeroDocumento == "1712345678")
            {
                return Task.FromResult(new ValidarDocumentoRecuperacionResponseDto
                {
                    Valido = true,
                    NombreCompleto = "Juan Perez",
                    Mensaje = "Documento verificado correctamente."
                });
            }

            return Task.FromResult(new ValidarDocumentoRecuperacionResponseDto
            {
                Valido = false,
                Mensaje = "Estimado estudiante, el Sistema de Titulación está habilitado exclusivamente para estudiantes del último semestre o egresados."
            });
        }

        public Task<ValidarFechaNacimientoResponseDto> ValidarFechaNacimientoAsync(ValidarFechaNacimientoRequestDto request, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new ValidarFechaNacimientoResponseDto
            {
                Valido = true,
                Mensaje = "Identidad validada exitosamente."
            });
        }

        public Task<RestablecerContrasenaResponseDto> RestablecerContrasenaAsync(RestablecerContrasenaRequestDto request, CancellationToken cancellationToken = default)
        {
            if (request.Token == "token-valido-prueba")
            {
                return Task.FromResult(new RestablecerContrasenaResponseDto
                {
                    Exito = true,
                    Mensaje = "Contraseña restablecida exitosamente."
                });
            }

            return Task.FromResult(new RestablecerContrasenaResponseDto
            {
                Exito = false,
                Mensaje = "El enlace o token de verificación es inválido o ha expirado."
            });
        }
    }
}
