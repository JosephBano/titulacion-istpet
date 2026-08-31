using TitulacionIstpet.Application.Features.Postulaciones.DTOs;

namespace TitulacionIstpet.Application.Features.Postulaciones.Comandos;

public sealed record CrearPostulacionComando(
    int IdMatricula,
    int IdModalidadTitulacionCarrera,
    IReadOnlyList<RequisitoPostulacionInputDto>? Requisitos
);

public sealed class CrearPostulacion(IRepositorioPostulaciones repositorio)
{
    private readonly IRepositorioPostulaciones _repositorio = repositorio;

    public async Task<int> EjecutarAsync(CrearPostulacionComando comando, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(comando);
        if (comando.IdMatricula <= 0)
        {
            throw new ArgumentException("El identificador de matrícula es inválido.", nameof(comando));
        }
        if (comando.IdModalidadTitulacionCarrera <= 0)
        {
            throw new ArgumentException("El identificador de la modalidad de titulación ofertada es inválido.", nameof(comando));
        }

        return await _repositorio.CrearPostulacionAsync(
            comando.IdMatricula,
            comando.IdModalidadTitulacionCarrera,
            comando.Requisitos,
            ct);
    }
}
