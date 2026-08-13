using FluentValidation.Results;

namespace TitulacionIstpet.Application.Common.Models;

/// <summary>Se traduce a HTTP 400 con el detalle por campo.</summary>
public class ValidacionException : Exception
{
    public ValidacionException(IEnumerable<ValidationFailure> fallos)
        : base("Se encontraron uno o mas errores de validacion.")
    {
        Errores = fallos
            .GroupBy(f => f.PropertyName, f => f.ErrorMessage)
            .ToDictionary(g => g.Key, g => g.ToArray());
    }

    public IDictionary<string, string[]> Errores { get; }
}
