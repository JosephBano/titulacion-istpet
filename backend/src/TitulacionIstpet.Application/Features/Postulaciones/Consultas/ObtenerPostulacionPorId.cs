using TitulacionIstpet.Application.Common.Models;
using TitulacionIstpet.Application.Features.Postulaciones.DTOs;

namespace TitulacionIstpet.Application.Features.Postulaciones.Consultas;

public sealed record ObtenerPostulacionPorIdConsulta(int IdPostulacionAlumnos);

public sealed class ObtenerPostulacionPorId(IRepositorioPostulaciones repositorio)
{
    private readonly IRepositorioPostulaciones _repositorio = repositorio;

    public async Task<PostulacionDetalleDto> EjecutarAsync(
        ObtenerPostulacionPorIdConsulta consulta, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(consulta);
        if (consulta.IdPostulacionAlumnos <= 0)
        {
            throw new ArgumentException("El identificador de la postulación es inválido.", nameof(consulta));
        }

        var postulacion = await _repositorio.ObtenerPorIdAsync(consulta.IdPostulacionAlumnos, ct);
        if (postulacion == null)
        {
            throw new NoEncontradoException("Postulación", consulta.IdPostulacionAlumnos);
        }

        return postulacion;
    }
}
