using TitulacionIstpet.Domain.Entities;

namespace TitulacionIstpet.Application.Auth;

public interface IRepositorioAutenticacion
{
    /// <summary>
    /// Usuario por su identificador SIGAFI, con el grafo RBAC necesario para resolver
    /// el acceso ya cargado. Devuelve null si no existe.
    /// </summary>
    Task<Usuario?> BuscarPorIdSigafiAsync(string idSigafi, CancellationToken ct = default);

    /// <summary>Asignaciones usuario-rol con toda la cadena de permisos cargada.</summary>
    Task<IReadOnlyList<RbacUsuarioRol>> ObtenerAsignacionesAsync(
        int idUsuario, CancellationToken ct = default);

    /// <summary>Regraba la contrasenia ya hasheada. Usado por la migracion progresiva.</summary>
    Task ActualizarContraseniaAsync(int idUsuario, string hash, CancellationToken ct = default);
}
