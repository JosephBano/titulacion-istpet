using TitulacionIstpet.Application.Features.Convocatorias.DTOs;

namespace TitulacionIstpet.Application.Features.Convocatorias.CasosDeUso;

public sealed class AperturarPeriodoConvocatoria(IRepositorioConvocatorias repo)
{
    private readonly IRepositorioConvocatorias _repo = repo;

    public Task<int> EjecutarAsync(AperturarPeriodoConvocatoriaComando comando, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(comando);
        if (string.IsNullOrWhiteSpace(comando.IdPeriodo))
        {
            throw new ArgumentException("El código de período lectivo es obligatorio.");
        }
        if (comando.FechaFinCorte <= comando.FechaInicioCorte)
        {
            throw new ArgumentException("La fecha de fin (corte) debe ser posterior a la fecha de inicio.");
        }

        return _repo.AperturarPeriodoConvocatoriaAsync(comando, ct);
    }
}

public sealed class ConsultarConvocatorias(IRepositorioConvocatorias repo)
{
    private readonly IRepositorioConvocatorias _repo = repo;

    public Task<ConvocatoriaDetalleDto?> ObtenerActivaAsync(CancellationToken ct = default)
        => _repo.ObtenerConvocatoriaActivaAsync(ct);

    public Task<ConvocatoriaDetalleDto?> ObtenerPorIdAsync(int idCohorte, CancellationToken ct = default)
        => _repo.ObtenerConvocatoriaPorIdAsync(idCohorte, ct);

    public Task<IReadOnlyList<ConvocatoriaResumenDto>> ListarAsync(CancellationToken ct = default)
        => _repo.ListarConvocatoriasAsync(ct);
}

public sealed class AdministrarConvocatoria(IRepositorioConvocatorias repo)
{
    private readonly IRepositorioConvocatorias _repo = repo;

    public Task AjustarFechasCorteAsync(AjustarFechasCorteComando comando, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(comando);
        return _repo.AjustarFechasCorteAsync(comando, ct);
    }

    public Task ConmutarModalidadCarreraAsync(ConmutarModalidadCarreraComando comando, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(comando);
        return _repo.ConmutarModalidadCarreraAsync(comando, ct);
    }
}
