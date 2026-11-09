namespace TitulacionIstpet.Application.DTOs.Pagos;

public sealed record ConfiguracionPagoModalidadDto(
    int IdConfiguracionPago,
    int IdModalidadTitulacion,
    string ModalidadNombre,
    string? IdPeriodo,
    decimal ValorBase,
    decimal MaximoInversion,
    decimal TotalTitulacion,
    int? IdEspecie,
    string? EspecieNombre,
    bool Activo,
    DateTime FechaRegistro,
    DateTime? FechaModificacion
);

public sealed record GuardarConfiguracionPagoModalidadDto(
    int IdModalidadTitulacion,
    string? IdPeriodo,
    decimal ValorBase,
    decimal MaximoInversion,
    decimal TotalTitulacion = 700.00m,
    int? IdEspecie = null,
    bool Activo = true
);
