namespace TitulacionIstpet.Application.Features.Postulaciones.Comandos;

public sealed record SolicitarCambioModalidadComando(
    int IdPostulacionAlumnos,
    int IdNuevaModalidadTitulacionCarrera
);

public sealed class SolicitarCambioModalidad(IRepositorioPostulaciones repositorio)
{
    private readonly IRepositorioPostulaciones _repositorio = repositorio;

    public async Task EjecutarAsync(SolicitarCambioModalidadComando comando, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(comando);
        if (comando.IdPostulacionAlumnos <= 0)
        {
            throw new ArgumentException("El identificador de la postulación es inválido.", nameof(comando));
        }
        if (comando.IdNuevaModalidadTitulacionCarrera <= 0)
        {
            throw new ArgumentException("El identificador de la nueva modalidad es inválido.", nameof(comando));
        }

        await _repositorio.SolicitarCambioModalidadAsync(
            comando.IdPostulacionAlumnos,
            comando.IdNuevaModalidadTitulacionCarrera,
            ct);
    }
}
