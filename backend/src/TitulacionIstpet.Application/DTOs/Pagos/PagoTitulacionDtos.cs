namespace TitulacionIstpet.Application.DTOs.Pagos;

public sealed record PagoTitulacionDto(
    int IdPagoTitulacion,
    int IdPostulacionAlumno,
    string EstudianteCedula,
    string EstudianteNombre,
    string CarreraNombre,
    string ModalidadNombre,
    int IdMatricula,
    string TipoRubro,
    string NumeroFactura,
    decimal Valor,
    string Estado,
    DateTime FechaRegistro,
    int IdUsuarioRegistro,
    string UsuarioRegistroNombre,
    string? Observacion,
    string? MotivoRechazo,
    DateTime? FechaRevision,
    int? IdUsuarioRevision,
    string? UsuarioRevisionNombre,
    string? ComprobanteArchivo
);

public sealed record RegistrarPagoTitulacionDto(
    int IdPostulacionAlumno,
    string TipoRubro,
    string NumeroFactura,
    decimal Valor,
    string? Observacion = null,
    string? ComprobanteArchivo = null
);

public sealed record RevisionPagoDto(
    bool Aprobar,
    string? MotivoRechazo = null,
    string? Observacion = null
);

public sealed record ResumenFinancieroEstudianteDto(
    int IdPostulacionAlumno,
    int IdMatricula,
    string EstudianteCedula,
    string EstudianteNombre,
    string CarreraNombre,
    string ModalidadNombre,
    int IdModalidadTitulacion,
    string? IdPeriodo,
    decimal ValorBaseMaximo,
    decimal InversionMaxima,
    decimal TotalTitulacion,
    decimal TotalBasePagado,
    decimal TotalInversionPagado,
    decimal TotalPagado,
    decimal SaldoBaseDisponible,
    decimal SaldoInversionDisponible,
    decimal SaldoTotalPendiente,
    IReadOnlyList<PagoTitulacionDto> Pagos
);

public sealed record HistorialCambioEstadoPagoDto(
    int IdPagoHistorial,
    int IdPagoTitulacion,
    string? EstadoAnterior,
    string EstadoNuevo,
    int IdUsuario,
    string UsuarioNombre,
    string? Motivo,
    DateTime FechaCambio
);
