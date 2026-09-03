using TitulacionIstpet.Application.Features.ResponsablesRequisitos.DTOs;

namespace TitulacionIstpet.Application.Features.ResponsablesRequisitos.Consultas;

public sealed record ListarPendientesDocenteConsulta(string IdProfesor);

public sealed class ListarPendientesDocente(IRepositorioResponsablesRequisitos repositorio)
{
    private readonly IRepositorioResponsablesRequisitos _repositorio = repositorio;

    public Task<IReadOnlyList<RequisitoEvaluacionDocenteDto>> EjecutarAsync(ListarPendientesDocenteConsulta consulta, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(consulta);
        if (string.IsNullOrWhiteSpace(consulta.IdProfesor))
        {
            throw new ArgumentException("La identificación del docente es requerida.", nameof(consulta));
        }

        return _repositorio.ListarPendientesDocenteAsync(consulta.IdProfesor.Trim(), ct);
    }
}
