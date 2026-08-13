namespace Titan.Domain.Exceptions;

/// <summary>
/// Regla de negocio violada. La API la traduce a HTTP 409 o ProblemDetails.
/// </summary>
public class DominioException : Exception
{
    public DominioException(string mensaje) : base(mensaje) { }
    public DominioException(string mensaje, Exception inner) : base(mensaje, inner) { }
}
