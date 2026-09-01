using TitulacionIstpet.Application.Features.Postulaciones.DTOs;

namespace TitulacionIstpet.Application.Features.Postulaciones.Comandos;

public sealed record ActualizarRequisitosPostulacionComando(
    int IdPostulacionAlumnos,
    IReadOnlyList<RequisitoPostulacionInputDto> Requisitos
);

public sealed class ActualizarRequisitosPostulacion(IRepositorioPostulaciones repositorio)
{
    private readonly IRepositorioPostulaciones _repositorio = repositorio;

    public async Task EjecutarAsync(ActualizarRequisitosPostulacionComando comando, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(comando);
        if (comando.IdPostulacionAlumnos <= 0)
        {
            throw new ArgumentException("El identificador de la postulación es inválido.", nameof(comando));
        }
        ArgumentNullException.ThrowIfNull(comando.Requisitos);

        await _repositorio.ActualizarRequisitosAsync(
            comando.IdPostulacionAlumnos,
            comando.Requisitos,
            ct);
    }
}
