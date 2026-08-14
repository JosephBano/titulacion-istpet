using FluentValidation;
using MediatR;
using TitulacionIstpet.Application.Common.Models;

namespace TitulacionIstpet.Application.Common.Behaviors;

/// <summary>
/// Corre todos los validadores registrados para el request antes de llegar al handler,
/// de modo que ningun handler tenga que revalidar su entrada.
/// </summary>
public class ValidationBehavior<TRequest, TResponse>(IEnumerable<IValidator<TRequest>> validadores) : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly IEnumerable<IValidator<TRequest>> _validadores = validadores;

    public async Task<TResponse> Handle(
        TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (!_validadores.Any())
        {
            return await next();
        }

        var contexto = new ValidationContext<TRequest>(request);
        var resultados = await Task.WhenAll(_validadores.Select(v => v.ValidateAsync(contexto, cancellationToken)));
        var fallos = resultados.SelectMany(r => r.Errors).Where(f => f is not null).ToList();

        if (fallos.Count != 0)
        {
            throw new ValidacionException(fallos);
        }

        return await next();
    }
}
