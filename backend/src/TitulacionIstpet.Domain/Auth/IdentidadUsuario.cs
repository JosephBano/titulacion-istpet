namespace TitulacionIstpet.Domain.Auth;

/// <summary>
/// Resultado de una autenticacion exitosa: quien es el usuario y que roles de
/// titulacion tiene. Solo se construye cuando el acceso ya fue concedido.
/// </summary>
public sealed record IdentidadUsuario(
    int IdUsuario,
    string IdSigafi,
    string? Nombre,
    IReadOnlySet<string> RolesTitulacion)
{
    public bool TieneRol(string codigoRol) => RolesTitulacion.Contains(codigoRol);
}
