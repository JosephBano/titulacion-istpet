using TitulacionIstpet.Application.Features.ConfiguracionGeneral.DTOs;

namespace TitulacionIstpet.Application.Features.ConfiguracionGeneral.CasosDeUso;

public sealed class ListarConfiguracionGeneral(IRepositorioConfiguracionGeneral repo)
{
    private readonly IRepositorioConfiguracionGeneral _repo = repo;

    public Task<IReadOnlyList<ModalidadMaestraDto>> ListarModalidadesAsync(bool soloActivas = false, CancellationToken ct = default)
        => _repo.ListarModalidadesAsync(soloActivas, ct);

    public Task<IReadOnlyList<RequisitoMaestroDto>> ListarRequisitosAsync(bool soloActivos = false, CancellationToken ct = default)
        => _repo.ListarRequisitosAsync(soloActivos, ct);

    public Task<IReadOnlyList<RequisitoModalidadMatrizDto>> ListarRequisitosPorModalidadAsync(int idModalidad, CancellationToken ct = default)
        => _repo.ListarMatrizPorModalidadAsync(idModalidad, ct);

    public Task<ResumenGeneralSistemaDto> ObtenerResumenGeneralAsync(CancellationToken ct = default)
        => _repo.ObtenerResumenGeneralAsync(ct);
}

public sealed class AdministrarModalidades(IRepositorioConfiguracionGeneral repo)
{
    private readonly IRepositorioConfiguracionGeneral _repo = repo;

    public Task<int> CrearAsync(CrearModalidadMaestraDto dto, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(dto);
        if (string.IsNullOrWhiteSpace(dto.ModalidadTitulacion))
        {
            throw new ArgumentException("El nombre de la modalidad de titulación es obligatorio.");
        }
        return _repo.CrearModalidadAsync(dto, ct);
    }

    public Task ActualizarAsync(ActualizarModalidadMaestraDto dto, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(dto);
        return _repo.ActualizarModalidadAsync(dto, ct);
    }

    public Task CambiarEstadoAsync(int idModalidad, bool activo, CancellationToken ct = default)
        => _repo.CambiarEstadoModalidadAsync(idModalidad, activo, ct);
}

public sealed class AdministrarRequisitos(IRepositorioConfiguracionGeneral repo)
{
    private readonly IRepositorioConfiguracionGeneral _repo = repo;

    public Task<int> CrearAsync(CrearRequisitoMaestroDto dto, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(dto);
        if (string.IsNullOrWhiteSpace(dto.Requisito))
        {
            throw new ArgumentException("El nombre del requisito es obligatorio.");
        }
        return _repo.CrearRequisitoAsync(dto, ct);
    }

    public Task ActualizarAsync(ActualizarRequisitoMaestroDto dto, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(dto);
        return _repo.ActualizarRequisitoAsync(dto, ct);
    }

    public Task CambiarEstadoAsync(int idRequisito, bool activo, CancellationToken ct = default)
        => _repo.CambiarEstadoRequisitoAsync(idRequisito, activo, ct);
}

public sealed class AdministrarMatrizRequisitosModalidad(IRepositorioConfiguracionGeneral repo)
{
    private readonly IRepositorioConfiguracionGeneral _repo = repo;

    public Task<int> AsignarAsync(AsignarRequisitoModalidadDto dto, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(dto);
        return _repo.AsignarRequisitoAModalidadAsync(dto, ct);
    }

    public Task DesasignarAsync(int idRequisitoModalidad, CancellationToken ct = default)
        => _repo.DesasignarRequisitoDeModalidadAsync(idRequisitoModalidad, ct);
}
