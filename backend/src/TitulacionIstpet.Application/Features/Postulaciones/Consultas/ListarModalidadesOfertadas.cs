using TitulacionIstpet.Application.Features.Postulaciones.DTOs;

namespace TitulacionIstpet.Application.Features.Postulaciones.Consultas;

public sealed record ListarModalidadesOfertadasConsulta(int IdCohorteCarrera);

public sealed class ListarModalidadesOfertadas(IRepositorioPostulaciones repositorio)
{
    private readonly IRepositorioPostulaciones _repositorio = repositorio;

    public Task<IReadOnlyList<ModalidadOfertadaDto>> EjecutarAsync(
        ListarModalidadesOfertadasConsulta consulta, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(consulta);
        if (consulta.IdCohorteCarrera <= 0)
        {
            throw new ArgumentException("El identificador de la cohorte-carrera es inválido.", nameof(consulta));
        }

        return _repositorio.ListarModalidadesOfertadasPorCohorteCarreraAsync(consulta.IdCohorteCarrera, ct);
    }
}
