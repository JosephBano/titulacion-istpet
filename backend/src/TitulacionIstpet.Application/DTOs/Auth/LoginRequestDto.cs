using System.ComponentModel.DataAnnotations;

namespace TitulacionIstpet.Application.DTOs.Auth;

public class LoginRequestDto
{
    [Required(ErrorMessage = "El nombre de usuario o correo institucional es requerido.")]
    public string UsernameOrEmail { get; set; } = string.Empty;

    [Required(ErrorMessage = "La contraseña es requerida.")]
    public string Password { get; set; } = string.Empty;

    public string SystemCode { get; set; } = "TITULACION";
}
