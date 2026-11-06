namespace TitulacionIstpet.Application.DTOs.Auth;

/// <summary>
/// Solicitud de validación de documento de identidad para recuperación de acceso
/// </summary>
public class ValidarDocumentoRecuperacionRequestDto
{
    public string NumeroDocumento { get; set; } = string.Empty;
}

/// <summary>
/// Respuesta del estado de habilitación de un documento de identidad para el sistema de titulación
/// </summary>
public class ValidarDocumentoRecuperacionResponseDto
{
    public bool Valido { get; set; }
    public string Mensaje { get; set; } = string.Empty;
    public string? NombreCompleto { get; set; }
    public bool RequiereFechaNacimiento { get; set; } = true;
}

/// <summary>
/// Solicitud para comprobar la fecha de nacimiento del usuario identificado
/// </summary>
public class ValidarFechaNacimientoRequestDto
{
    public string NumeroDocumento { get; set; } = string.Empty;
    public DateOnly FechaNacimiento { get; set; }
}

/// <summary>
/// Respuesta de comprobación de fecha de nacimiento
/// </summary>
public class ValidarFechaNacimientoResponseDto
{
    public bool Valido { get; set; }
    public string Mensaje { get; set; } = string.Empty;
    public string? Token { get; set; }
}

/// <summary>
/// Solicitud de restablecimiento de contraseña tras verificación de identidad
/// </summary>
public class RestablecerContrasenaRequestDto
{
    public string NumeroDocumento { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
    public string NuevaContrasenia { get; set; } = string.Empty;
    public string ConfirmarContrasenia { get; set; } = string.Empty;
}

/// <summary>
/// Respuesta al restablecimiento de contraseña
/// </summary>
public class RestablecerContrasenaResponseDto
{
    public bool Exito { get; set; }
    public string Mensaje { get; set; } = string.Empty;
}
