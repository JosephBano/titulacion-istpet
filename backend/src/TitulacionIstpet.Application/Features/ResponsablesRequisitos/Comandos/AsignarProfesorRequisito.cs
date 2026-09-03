using TitulacionIstpet.Application.Features.ResponsablesRequisitos.DTOs;

namespace TitulacionIstpet.Application.Features.ResponsablesRequisitos.Comandos;

public sealed record AsignarProfesorRequisitoComando(int IdRequisitos, string IdProfesor);

public sealed class AsignarProfesorRequisito(IRepositorioResponsablesRequisitos repositorio)
{
    private readonly IRepositorioResponsablesRequisitos _repositorio = repositorio;

    public Task<int> EjecutarAsync(AsignarProfesorRequisitoComando comando, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(comando);
        if (comando.IdRequisitos <= 0)
        {
            throw new ArgumentException("El identificador del requisito es inválido.", nameof(comando));
        }

        if (string.IsNullOrWhiteSpace(comando.IdProfesor))
        {
            throw new ArgumentException("La identificación del docente es requerida.", nameof(comando));
        }

        return _repositorio.AsignarProfesorAsync(comando.IdRequisitos, comando.IdProfesor.Trim(), ct);
    }
}
