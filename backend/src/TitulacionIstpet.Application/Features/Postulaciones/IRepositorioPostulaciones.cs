using TitulacionIstpet.Application.Features.Postulaciones.DTOs;

namespace TitulacionIstpet.Application.Features.Postulaciones;

public interface IRepositorioPostulaciones
{
    Task<ElegibilidadPostulacionDto> ObtenerElegibilidadEstudianteAsync(string idAlumno, CancellationToken ct = default);
    Task<IReadOnlyList<ModalidadOfertadaDto>> ListarModalidadesOfertadasPorCohorteCarreraAsync(int idCohorteCarrera, CancellationToken ct = default);
    Task<PostulacionDetalleDto?> ObtenerMiPostulacionActivaAsync(string idAlumno, CancellationToken ct = default);
    Task<PostulacionDetalleDto?> ObtenerPorIdAsync(int idPostulacionAlumnos, CancellationToken ct = default);
    Task<PaginaPostulacionesDto> ListarPostulacionesAsync(
        int? idCohorte,
        int? idCarrera,
        int? idModalidad,
        int? idEstado,
        string? busqueda,
        int pagina,
        int tamanoPagina,
        CancellationToken ct = default);
    Task<IReadOnlyList<EstadoPostulacionDto>> ListarEstadosAsync(CancellationToken ct = default);
    Task<int> CrearPostulacionAsync(
        int idMatricula,
        int idModalidadTitulacionCarrera,
        IReadOnlyList<RequisitoPostulacionInputDto>? requisitos,
        CancellationToken ct = default);
    Task ActualizarRequisitosAsync(
        int idPostulacionAlumnos,
        IReadOnlyList<RequisitoPostulacionInputDto> requisitos,
        CancellationToken ct = default);
    Task CambiarEstadoAsync(
        int idPostulacionAlumnos,
        int idNuevoEstado,
        CancellationToken ct = default);
    Task SolicitarCambioModalidadAsync(
        int idPostulacionAlumnos,
        int idNuevaModalidadTitulacionCarrera,
        CancellationToken ct = default);
    Task<PortalEstudianteDto> ObtenerPortalEstudianteAsync(string idAlumno, CancellationToken ct = default);
    Task DictaminarPostulacionAsync(DictamenPostulacionComando comando, CancellationToken ct = default);
}
