namespace TitulacionIstpet.Application.DTOs.Auth;

public record RbacSistemaDto(
    int IdSistema,
    string? Codigo,
    string? Detalle,
    string? Icono
);

public record RbacOperacionDto(
    int IdModulosOperaciones,
    int IdOperacion,
    string? NombreOperacion
);

public record RbacModuloDto(
    int IdModulo,
    string? Nombre,
    bool? EsActivo,
    List<RbacOperacionDto> Operaciones
);

public record RbacRolDto(
    int IdRol,
    string? Nombre,
    string? CodigoRol,
    bool? EsActivo
);
