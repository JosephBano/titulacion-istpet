using TitulacionIstpet.Application.Features.Convocatorias.DTOs;

namespace TitulacionIstpet.Application.Features.Convocatorias;

public interface IRepositorioConvocatorias
{
    Task<int> AperturarPeriodoConvocatoriaAsync(AperturarPeriodoConvocatoriaComando comando, CancellationToken ct = default);
    Task<ConvocatoriaDetalleDto?> ObtenerConvocatoriaActivaAsync(CancellationToken ct = default);
    Task<ConvocatoriaDetalleDto?> ObtenerConvocatoriaPorIdAsync(int idCohorte, CancellationToken ct = default);
    Task<IReadOnlyList<ConvocatoriaResumenDto>> ListarConvocatoriasAsync(CancellationToken ct = default);
    Task AjustarFechasCorteAsync(AjustarFechasCorteComando comando, CancellationToken ct = default);
    Task ConmutarModalidadCarreraAsync(ConmutarModalidadCarreraComando comando, CancellationToken ct = default);
}
