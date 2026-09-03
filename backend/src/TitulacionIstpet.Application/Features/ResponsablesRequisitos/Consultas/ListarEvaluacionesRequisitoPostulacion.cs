using TitulacionIstpet.Application.Features.ResponsablesRequisitos.DTOs;

namespace TitulacionIstpet.Application.Features.ResponsablesRequisitos.Consultas;

public sealed class ListarEvaluacionesRequisitoPostulacion(IRepositorioResponsablesRequisitos repositorio)
{
    private readonly IRepositorioResponsablesRequisitos _repositorio = repositorio;

    public Task<IReadOnlyList<EvaluacionDocenteItemDto>> EjecutarAsync(int idPostulacionAlumnoRequisitoModalidad, CancellationToken ct = default)
    {
        return _repositorio.ListarEvaluacionesPorRequisitoPostulacionAsync(idPostulacionAlumnoRequisitoModalidad, ct);
    }
}
