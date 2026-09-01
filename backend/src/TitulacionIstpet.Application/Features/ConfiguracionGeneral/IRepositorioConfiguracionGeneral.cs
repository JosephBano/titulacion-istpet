using TitulacionIstpet.Application.Features.ConfiguracionGeneral.DTOs;

namespace TitulacionIstpet.Application.Features.ConfiguracionGeneral;

public interface IRepositorioConfiguracionGeneral
{
    // Modalidades
    Task<IReadOnlyList<ModalidadMaestraDto>> ListarModalidadesAsync(bool soloActivas = false, CancellationToken ct = default);
    Task<ModalidadMaestraDto?> ObtenerModalidadPorIdAsync(int idModalidad, CancellationToken ct = default);
    Task<int> CrearModalidadAsync(CrearModalidadMaestraDto dto, CancellationToken ct = default);
    Task ActualizarModalidadAsync(ActualizarModalidadMaestraDto dto, CancellationToken ct = default);
    Task CambiarEstadoModalidadAsync(int idModalidad, bool activo, CancellationToken ct = default);

    // Requisitos
    Task<IReadOnlyList<RequisitoMaestroDto>> ListarRequisitosAsync(bool soloActivos = false, CancellationToken ct = default);
    Task<RequisitoMaestroDto?> ObtenerRequisitoPorIdAsync(int idRequisito, CancellationToken ct = default);
    Task<int> CrearRequisitoAsync(CrearRequisitoMaestroDto dto, CancellationToken ct = default);
    Task ActualizarRequisitoAsync(ActualizarRequisitoMaestroDto dto, CancellationToken ct = default);
    Task CambiarEstadoRequisitoAsync(int idRequisito, bool activo, CancellationToken ct = default);

    // Matriz Requisito - Modalidad
    Task<IReadOnlyList<RequisitoModalidadMatrizDto>> ListarMatrizPorModalidadAsync(int idModalidad, CancellationToken ct = default);
    Task<int> AsignarRequisitoAModalidadAsync(AsignarRequisitoModalidadDto dto, CancellationToken ct = default);
    Task DesasignarRequisitoDeModalidadAsync(int idRequisitoModalidad, CancellationToken ct = default);

    // Resumen General del Sistema
    Task<ResumenGeneralSistemaDto> ObtenerResumenGeneralAsync(CancellationToken ct = default);
}
