using TitulacionIstpet.Application.Features.Postulaciones.DTOs;

namespace TitulacionIstpet.Application.Features.Postulaciones.Comandos;

public sealed class DictaminarPostulacion(IRepositorioPostulaciones repositorio)
{
    private readonly IRepositorioPostulaciones _repositorio = repositorio;

    public Task EjecutarAsync(DictamenPostulacionComando comando, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(comando);
        if (comando.IdPostulacionAlumnos <= 0)
        {
            throw new ArgumentException("El identificador de la postulación es requerido.", nameof(comando));
        }

        var decision = comando.Decision?.Trim().ToUpperInvariant();
        if (decision != "APROBAR" && decision != "OBSERVAR" && decision != "RECHAZAR")
        {
            throw new ArgumentException("La decisión debe ser 'APROBAR', 'OBSERVAR' o 'RECHAZAR'.", nameof(comando));
        }

        return _repositorio.DictaminarPostulacionAsync(comando with { Decision = decision }, ct);
    }
}
