namespace TitulacionIstpet.Application.Features.ResponsablesRequisitos.Comandos;

public sealed class DesasignarProfesorRequisito(IRepositorioResponsablesRequisitos repositorio)
{
    private readonly IRepositorioResponsablesRequisitos _repositorio = repositorio;

    public Task EjecutarAsync(int idResponsableEvidencias, CancellationToken ct = default)
    {
        if (idResponsableEvidencias <= 0)
        {
            throw new ArgumentException("El identificador de la asignación es inválido.", nameof(idResponsableEvidencias));
        }

        return _repositorio.DesasignarProfesorAsync(idResponsableEvidencias, ct);
    }
}
