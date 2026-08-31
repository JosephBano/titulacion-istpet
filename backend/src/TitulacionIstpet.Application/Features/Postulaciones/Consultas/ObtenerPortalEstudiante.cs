using TitulacionIstpet.Application.Features.Postulaciones.DTOs;

namespace TitulacionIstpet.Application.Features.Postulaciones.Consultas;

public sealed record ObtenerPortalEstudianteConsulta(string IdAlumno);

public sealed class ObtenerPortalEstudiante(IRepositorioPostulaciones repositorio)
{
    private readonly IRepositorioPostulaciones _repositorio = repositorio;

    public Task<PortalEstudianteDto> EjecutarAsync(
        ObtenerPortalEstudianteConsulta consulta, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(consulta);
        if (string.IsNullOrWhiteSpace(consulta.IdAlumno))
        {
            throw new ArgumentException("El identificador del estudiante es requerido.", nameof(consulta));
        }

        return _repositorio.ObtenerPortalEstudianteAsync(consulta.IdAlumno.Trim(), ct);
    }
}
