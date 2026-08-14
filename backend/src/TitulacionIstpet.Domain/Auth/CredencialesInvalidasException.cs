namespace TitulacionIstpet.Domain.Auth;

/// <summary>
/// Falla de autenticacion. El mensaje es deliberadamente generico y unico para todos
/// los casos —usuario inexistente, contrasenia incorrecta, cuenta inactiva, sin
/// permisos sobre titulacion—: distinguirlos le confirmaria a un atacante que
/// cuentas existen.
/// </summary>
public class CredencialesInvalidasException : Exception
{
    public const string MensajeGenerico = "Usuario o contrasenia incorrectos.";

    public CredencialesInvalidasException() : base(MensajeGenerico) { }

    public CredencialesInvalidasException(string mensaje) : base(mensaje) { }

    public CredencialesInvalidasException(string mensaje, Exception inner)
        : base(mensaje, inner) { }
}
