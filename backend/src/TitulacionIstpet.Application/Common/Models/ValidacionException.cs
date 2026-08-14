using FluentValidation.Results;

namespace TitulacionIstpet.Application.Common.Models;

/// <summary>Se traduce a HTTP 400 con el detalle por campo.</summary>
public class ValidacionException(IEnumerable<ValidationFailure> fallos) : Exception("Se encontraron uno o mas errores de validacion.")
{
    public IDictionary<string, string[]> Errores { get; } = fallos
            .GroupBy(f => f.PropertyName, f => f.ErrorMessage)
            .ToDictionary(g => g.Key, g => g.ToArray());
}
