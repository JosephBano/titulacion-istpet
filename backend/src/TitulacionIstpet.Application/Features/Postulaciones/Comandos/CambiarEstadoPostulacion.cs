namespace TitulacionIstpet.Application.Features.Postulaciones.Comandos;

public sealed record CambiarEstadoPostulacionComando(
    int IdPostulacionAlumnos,
    int IdNuevoEstado
);

public sealed class CambiarEstadoPostulacion(IRepositorioPostulaciones repositorio)
{
    private readonly IRepositorioPostulaciones _repositorio = repositorio;

    public async Task EjecutarAsync(CambiarEstadoPostulacionComando comando, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(comando);
        if (comando.IdPostulacionAlumnos <= 0)
        {
            throw new ArgumentException("El identificador de la postulación es inválido.", nameof(comando));
        }
        if (comando.IdNuevoEstado <= 0)
        {
            throw new ArgumentException("El identificador del nuevo estado es inválido.", nameof(comando));
        }

        await _repositorio.CambiarEstadoAsync(
            comando.IdPostulacionAlumnos,
            comando.IdNuevoEstado,
            ct);
    }
}
