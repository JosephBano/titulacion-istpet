namespace Titan.Application.Common.Models;

/// <summary>Se traduce a HTTP 404.</summary>
public class NoEncontradoException : Exception
{
    public NoEncontradoException(string entidad, object clave)
        : base($"No se encontro {entidad} con clave '{clave}'.") { }
}
