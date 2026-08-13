namespace TitulacionIstpet.Domain.Exceptions;

/// <summary>
/// Regla de negocio violada. La WebApi la traduce a HTTP 409 en el middleware de errores.
/// </summary>
public class DominioException : Exception
{
    public DominioException(string mensaje) : base(mensaje) { }
    public DominioException(string mensaje, Exception inner) : base(mensaje, inner) { }
}
