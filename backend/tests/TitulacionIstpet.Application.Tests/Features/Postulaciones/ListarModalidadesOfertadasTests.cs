using FluentAssertions;
using NSubstitute;
using TitulacionIstpet.Application.Features.Postulaciones;
using TitulacionIstpet.Application.Features.Postulaciones.Consultas;
using TitulacionIstpet.Application.Features.Postulaciones.DTOs;
using Xunit;

namespace TitulacionIstpet.Application.Tests.Features.Postulaciones;

public class ListarModalidadesOfertadasTests
{
    private readonly IRepositorioPostulaciones _repo = Substitute.For<IRepositorioPostulaciones>();
    private readonly ListarModalidadesOfertadas _sut;

    public ListarModalidadesOfertadasTests()
    {
        _sut = new ListarModalidadesOfertadas(_repo);
    }

    [Fact]
    public async Task Consulta_con_id_valido_invoca_repositorio_y_retorna_lista()
    {
        var listaEsperada = new List<ModalidadOfertadaDto>
        {
            new(
                IdModalidadTitulacionCarrera: 1,
                IdModalidadTitulacion: 1,
                ModalidadTitulacion: "Examen Complexivo",
                EsComplexivo: "1",
                EsArticuloCientifico: "0",
                GeneraTesis: "0",
                Requisitos: new List<RequisitoModalidadOfertadaDto>()
            )
        };

        _repo.ListarModalidadesOfertadasPorCohorteCarreraAsync(5, Arg.Any<CancellationToken>())
            .Returns(listaEsperada);

        var resultado = await _sut.EjecutarAsync(new ListarModalidadesOfertadasConsulta(5));

        resultado.Should().BeEquivalentTo(listaEsperada);
        await _repo.Received(1).ListarModalidadesOfertadasPorCohorteCarreraAsync(5, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Id_cohorte_carrera_invalido_lanza_ArgumentException()
    {
        var accion = () => _sut.EjecutarAsync(new ListarModalidadesOfertadasConsulta(0));

        await accion.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*identificador de la cohorte-carrera*");
    }
}
