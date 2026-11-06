using TitulacionIstpet.Application.DTOs.Pagos;

namespace TitulacionIstpet.Application.Interfaces;

public interface IPagosTitulacionService
{
    Task<IReadOnlyList<ConfiguracionPagoModalidadDto>> ListarConfiguracionesAsync(string? idPeriodo = null, CancellationToken ct = default);
    Task<ConfiguracionPagoModalidadDto?> ObtenerConfiguracionPorModalidadAsync(int idModalidadTitulacion, string? idPeriodo = null, CancellationToken ct = default);
    Task<ConfiguracionPagoModalidadDto> GuardarConfiguracionAsync(GuardarConfiguracionPagoModalidadDto dto, int idUsuario, CancellationToken ct = default);

    Task<ResumenFinancieroEstudianteDto> ObtenerResumenFinancieroEstudianteAsync(int idPostulacionAlumno, CancellationToken ct = default);
    Task<PagoTitulacionDto> RegistrarPagoAsync(RegistrarPagoTitulacionDto dto, int idUsuario, CancellationToken ct = default);

    Task<IReadOnlyList<PagoTitulacionDto>> ListarBandejaContabilidadAsync(string? estado = null, CancellationToken ct = default);
    Task<PagoTitulacionDto> ProcesarRevisionContabilidadAsync(int idPagoTitulacion, RevisionPagoDto revision, int idUsuarioContabilidad, CancellationToken ct = default);
    Task<IReadOnlyList<HistorialCambioEstadoPagoDto>> ObtenerHistorialPagoAsync(int idPagoTitulacion, CancellationToken ct = default);
}
