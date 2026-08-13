using FluentAssertions;
using NSubstitute;
using Titan.Application.Common.Interfaces;
using Titan.Application.Features.Estudiantes.Commands;
using Titan.Domain.Entities;
using Titan.Domain.Exceptions;
using Titan.Domain.Repositories;
using Xunit;

namespace Titan.Application.Tests;

public class CrearEstudianteHandlerTests
{
    private readonly IEstudianteRepository _repo = Substitute.For<IEstudianteRepository>();
    private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();

    private static readonly CrearEstudianteCommand Comando =
        new("1712345678", "Ana", "Perez", "ana.perez@istpet.edu.ec");

    [Fact]
    public async Task Persiste_y_guarda_cuando_la_cedula_es_nueva()
    {
        _repo.ObtenerPorCedulaAsync(Comando.Cedula, Arg.Any<CancellationToken>())
            .Returns((Estudiante?)null);
        var handler = new CrearEstudianteHandler(_repo, _uow);

        await handler.Handle(Comando, CancellationToken.None);

        await _repo.Received(1).AgregarAsync(Arg.Any<Estudiante>(), Arg.Any<CancellationToken>());
        await _uow.Received(1).GuardarCambiosAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Rechaza_cedula_duplicada_sin_guardar()
    {
        _repo.ObtenerPorCedulaAsync(Comando.Cedula, Arg.Any<CancellationToken>())
            .Returns(new Estudiante(Comando.Cedula, "Otro", "Nombre", "otro@istpet.edu.ec"));
        var handler = new CrearEstudianteHandler(_repo, _uow);

        var accion = () => handler.Handle(Comando, CancellationToken.None);

        await accion.Should().ThrowAsync<DominioException>();
        await _uow.DidNotReceive().GuardarCambiosAsync(Arg.Any<CancellationToken>());
    }
}
