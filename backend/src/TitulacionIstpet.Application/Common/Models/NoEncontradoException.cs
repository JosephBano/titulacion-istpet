namespace TitulacionIstpet.Application.Common.Models;

/// <summary>Se traduce a HTTP 404.</summary>
public class NoEncontradoException(string entidad, object clave) : Exception($"No se encontro {entidad} con clave '{clave}'.")
{
}
