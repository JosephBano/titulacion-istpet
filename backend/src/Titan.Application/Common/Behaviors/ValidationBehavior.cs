using FluentValidation;
using MediatR;
using Titan.Application.Common.Models;

namespace Titan.Application.Common.Behaviors;

public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly IEnumerable<IValidator<TRequest>> _validadores;

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validadores) => _validadores = validadores;

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
