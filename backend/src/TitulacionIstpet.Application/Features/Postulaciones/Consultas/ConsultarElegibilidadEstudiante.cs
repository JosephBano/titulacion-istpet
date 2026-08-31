using TitulacionIstpet.Application.Features.Postulaciones.DTOs;

namespace TitulacionIstpet.Application.Features.Postulaciones.Consultas;

public sealed record ConsultarElegibilidadEstudianteConsulta(string IdAlumno);

public sealed class ConsultarElegibilidadEstudiante(IRepositorioPostulaciones repositorio)
{
    private readonly IRepositorioPostulaciones _repositorio = repositorio;

    public Task<ElegibilidadPostulacionDto> EjecutarAsync(
        ConsultarElegibilidadEstudianteConsulta consulta, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(consulta);
        if (string.IsNullOrWhiteSpace(consulta.IdAlumno))
        {
            throw new ArgumentException("El identificador del estudiante es requerido.", nameof(consulta));
        }

        return _repositorio.ObtenerElegibilidadEstudianteAsync(consulta.IdAlumno.Trim(), ct);
    }
}
