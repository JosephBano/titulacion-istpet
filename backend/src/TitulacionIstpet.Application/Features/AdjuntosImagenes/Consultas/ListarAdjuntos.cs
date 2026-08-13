namespace TitulacionIstpet.Application.Features.AdjuntosImagenes.Consultas;

public sealed record ListarAdjuntosConsulta(int Pagina = 1, int TamanoPagina = 20);

public sealed record PaginaAdjuntos(
    IReadOnlyList<AdjuntosImageneDto> Items,
    int Pagina,
    int TamanoPagina,
    int Total);

/// <summary>
/// Listado paginado de adjuntos. El tamano de pagina se acota en el caso
/// de uso (no en el controlador) para que cualquier consumidor —REST, un
/// job, otro handler— reciba siempre el mismo limite maximo.
/// </summary>
public sealed class ListarAdjuntos
{
    public const int TamanoPaginaPorDefecto = 20;
    public const int TamanoPaginaMaximo = 200;

    private readonly IRepositorioAdjuntosImagenes _repositorio;

    public ListarAdjuntos(IRepositorioAdjuntosImagenes repositorio)
    {
        _repositorio = repositorio;
    }

    public async Task<PaginaAdjuntos> EjecutarAsync(
        ListarAdjuntosConsulta consulta, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(consulta);

        var pagina = consulta.Pagina < 1 ? 1 : consulta.Pagina;
        var tamano = consulta.TamanoPagina switch
        {
            < 1 => TamanoPaginaPorDefecto,
            > TamanoPaginaMaximo => TamanoPaginaMaximo,
            _ => consulta.TamanoPagina
        };

        var entidades = await _repositorio.ListarAsync(pagina, tamano, ct);
        var total = await _repositorio.ContarAsync(ct);

        var items = entidades
            .Select(AdjuntosImageneMapeo.A_DTO)
            .ToList();

        return new PaginaAdjuntos(items, pagina, tamano, total);
    }
}
