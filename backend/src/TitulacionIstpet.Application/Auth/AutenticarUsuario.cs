using TitulacionIstpet.Domain.Auth;

namespace TitulacionIstpet.Application.Auth;

/// <summary>
/// Caso de uso de login. Cuatro puertas en orden, y todas fallan con el mismo mensaje
/// generico para no revelar cual se cerro:
///
///   1. El usuario existe.
///   2. La cuenta esta activa.
///   3. La contrasenia coincide (BCrypt o texto plano legacy).
///   4. Tiene al menos un rol de titulacion vigente.
///
/// La cuarta es la que aisla este sistema: un usuario perfectamente valido en
/// gestion academica, con credenciales correctas sobre la misma tabla compartida,
/// no entra aqui si no tiene permisos sobre 'titl'.
/// </summary>
public sealed class AutenticarUsuario
{
    private readonly IRepositorioAutenticacion _repositorio;
    private readonly IVerificadorCredenciales _verificador;

    public AutenticarUsuario(
        IRepositorioAutenticacion repositorio, IVerificadorCredenciales verificador)
    {
        _repositorio = repositorio;
        _verificador = verificador;
    }

    public async Task<IdentidadUsuario> EjecutarAsync(
        string? idSigafi, string? contrasenia, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(idSigafi) || string.IsNullOrEmpty(contrasenia))
        {
            throw new CredencialesInvalidasException();
        }

        var usuario = await _repositorio.BuscarPorIdSigafiAsync(idSigafi.Trim(), ct)
            ?? throw new CredencialesInvalidasException();

        if (!usuario.Activo)
        {
            throw new CredencialesInvalidasException();
        }

        var resultado = _verificador.Verificar(contrasenia, usuario.Contrasenia);
        if (!resultado.EsValida)
        {
            throw new CredencialesInvalidasException();
        }

        var asignaciones = await _repositorio.ObtenerAsignacionesAsync(usuario.IdUsuario, ct);
        var roles = AccesoTitulacion.ResolverRoles(asignaciones);

        // Fail-closed: sin roles de titulacion no hay acceso. Deliberadamente distinto
        // del AuthService de auth_global, que ante cero permisos concede todos los sistemas.
        if (roles.Count == 0)
        {
            throw new CredencialesInvalidasException();
        }

        // La migracion de la contrasenia ocurre despues de conceder el acceso: si algo
        // falla al grabar, el usuario ya se autentico y no se le bloquea la entrada.
        if (resultado.RequiereRehash)
        {
            await MigrarContraseniaAsync(usuario.IdUsuario, contrasenia, ct);
        }

        return new IdentidadUsuario(
            usuario.IdUsuario, usuario.IdSigafi, usuario.Nombre, roles);
    }

    private async Task MigrarContraseniaAsync(int idUsuario, string enClaro, CancellationToken ct)
    {
        try
        {
            var hash = _verificador.Hashear(enClaro);
            await _repositorio.ActualizarContraseniaAsync(idUsuario, hash, ct);
        }
        catch (Exception) when (!ct.IsCancellationRequested)
        {
            // Un fallo migrando no puede negar un login ya valido. Se reintentara en el
            // proximo ingreso; la contrasenia legacy sigue funcionando mientras tanto.
        }
    }
}
