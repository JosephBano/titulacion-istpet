using TitulacionIstpet.Application.Features.Postulaciones.DTOs;

namespace TitulacionIstpet.Application.Features.Postulaciones.Consultas;

public sealed record ListarPostulacionesConsulta(
    int? IdCohorte = null,
    int? IdCarrera = null,
    int? IdModalidad = null,
    int? IdEstado = null,
    string? Busqueda = null,
    int Pagina = 1,
    int TamanoPagina = 20
);

public sealed class ListarPostulaciones(IRepositorioPostulaciones repositorio)
{
    public const int TamanoPaginaPorDefecto = 20;
    public const int TamanoPaginaMaximo = 100;

    private readonly IRepositorioPostulaciones _repositorio = repositorio;

    public Task<PaginaPostulacionesDto> EjecutarAsync(
        ListarPostulacionesConsulta consulta, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(consulta);

        int pagina = consulta.Pagina < 1 ? 1 : consulta.Pagina;
        int tamano = consulta.TamanoPagina switch
        {
            < 1 => TamanoPaginaPorDefecto,
            > TamanoPaginaMaximo => TamanoPaginaMaximo,
            _ => consulta.TamanoPagina
        };

        return _repositorio.ListarPostulacionesAsync(
            consulta.IdCohorte,
            consulta.IdCarrera,
            consulta.IdModalidad,
            consulta.IdEstado,
            consulta.Busqueda?.Trim(),
            pagina,
            tamano,
            ct);
    }
}
