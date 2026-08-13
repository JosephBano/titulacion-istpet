using FluentValidation;
using MediatR;
using TitulacionIstpet.Application.Common.Interfaces;
using TitulacionIstpet.Domain.Entities;
using TitulacionIstpet.Domain.Exceptions;
using TitulacionIstpet.Domain.Repositories;

namespace TitulacionIstpet.Application.Features.Estudiantes.Commands;

public record CrearEstudianteCommand(
    string Cedula,
    string Nombres,
    string Apellidos,
    string CorreoInstitucional) : IRequest<int>;

public class CrearEstudianteValidator : AbstractValidator<CrearEstudianteCommand>
{
    public CrearEstudianteValidator()
    {
        // Cedula ecuatoriana: 10 digitos.
        RuleFor(x => x.Cedula).NotEmpty().Matches(@"^\d{10}$")
            .WithMessage("La cedula debe tener 10 digitos.");
        RuleFor(x => x.Nombres).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Apellidos).NotEmpty().MaximumLength(100);
        RuleFor(x => x.CorreoInstitucional).NotEmpty().EmailAddress().MaximumLength(180);
    }
}

public class CrearEstudianteHandler : IRequestHandler<CrearEstudianteCommand, int>
{
    private readonly IEstudianteRepository _repo;
    private readonly IUnitOfWork _uow;

    public CrearEstudianteHandler(IEstudianteRepository repo, IUnitOfWork uow)
    {
        _repo = repo;
        _uow = uow;
    }

    public async Task<int> Handle(CrearEstudianteCommand request, CancellationToken cancellationToken)
    {
        if (await _repo.ObtenerPorCedulaAsync(request.Cedula, cancellationToken) is not null)
        {
            throw new DominioException($"Ya existe un estudiante con cedula {request.Cedula}.");
        }

        var estudiante = new Estudiante(
            request.Cedula, request.Nombres, request.Apellidos, request.CorreoInstitucional);

        await _repo.AgregarAsync(estudiante, cancellationToken);
        await _uow.GuardarCambiosAsync(cancellationToken);

        return estudiante.Id;
    }
}
