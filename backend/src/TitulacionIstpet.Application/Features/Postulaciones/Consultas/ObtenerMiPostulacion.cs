using TitulacionIstpet.Application.Features.Postulaciones.DTOs;

namespace TitulacionIstpet.Application.Features.Postulaciones.Consultas;

public sealed record ObtenerMiPostulacionConsulta(string IdAlumno);

public sealed class ObtenerMiPostulacion(IRepositorioPostulaciones repositorio)
{
    private readonly IRepositorioPostulaciones _repositorio = repositorio;

    public async Task<PostulacionDetalleDto?> EjecutarAsync(
        ObtenerMiPostulacionConsulta consulta, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(consulta);
        if (string.IsNullOrWhiteSpace(consulta.IdAlumno))
        {
            throw new ArgumentException("El identificador del estudiante es requerido.", nameof(consulta));
        }

        return await _repositorio.ObtenerMiPostulacionActivaAsync(consulta.IdAlumno.Trim(), ct);
    }
}
