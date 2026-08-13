using TitulacionIstpet.Application.Auth;

namespace TitulacionIstpet.Infrastructure.Auth;

/// <summary>
/// Verificacion de contrasenias compatible con el resto del ecosistema ISTPET.
///
/// La tabla `usuarios` es compartida por varios sistemas y contiene contrasenias en
/// dos formatos a la vez, resultado de una migracion todavia inconclusa:
///
///   - BCrypt: prefijo '$2', work factor 11. Es el formato canonico.
///   - Texto plano: filas antiguas que nunca se migraron.
///
/// Ambos deben seguir funcionando: si titulacion rechazara las de texto plano, esos
/// usuarios no podrian entrar aqui pero si en los demas sistemas. Cuando una
/// contrasenia en claro se valida, el resultado marca RequiereRehash para que la capa
/// superior la regrabe hasheada; asi la migracion avanza sola con cada login.
///
/// El work factor debe coincidir con auth_global (PasswordService.WorkFactor = 11).
/// Un valor distinto genera hashes que el resto del ecosistema igual puede verificar
/// —BCrypt guarda el coste dentro del hash— pero rompe la homogeneidad del costo.
/// </summary>
public sealed class VerificadorCredencialesBcrypt : IVerificadorCredenciales
{
    /// <summary>Debe mantenerse sincronizado con auth_global_istpet PasswordService.</summary>
    public const int WorkFactor = 11;

    private const string PrefijoBcrypt = "$2";

    public bool EsHash(string? almacenado) =>
        !string.IsNullOrEmpty(almacenado)
        && almacenado.Length >= 4
        && almacenado.StartsWith(PrefijoBcrypt, StringComparison.Ordinal);

    public string Hashear(string contraseniaEnClaro)
    {
        ArgumentException.ThrowIfNullOrEmpty(contraseniaEnClaro);
        return BCrypt.Net.BCrypt.HashPassword(contraseniaEnClaro, WorkFactor);
    }

    public ResultadoVerificacion Verificar(string? contraseniaEnClaro, string? almacenado)
    {
        if (string.IsNullOrEmpty(contraseniaEnClaro) || string.IsNullOrEmpty(almacenado))
        {
            return ResultadoVerificacion.Fallida;
        }

        if (EsHash(almacenado))
        {
            return VerificarHash(contraseniaEnClaro, almacenado);
        }

        return VerificarTextoPlano(contraseniaEnClaro, almacenado);
    }

    private static ResultadoVerificacion VerificarHash(string enClaro, string hash)
    {
        try
        {
            return BCrypt.Net.BCrypt.Verify(enClaro, hash)
                ? ResultadoVerificacion.Valida()
                : ResultadoVerificacion.Fallida;
        }
        catch (Exception ex) when (ex is BCrypt.Net.SaltParseException or ArgumentException)
        {
            // Hash corrupto o truncado en la base. Se trata como credencial invalida y no
            // como error: un fallo aqui no debe tumbar el login ni filtrar el estado del dato.
            //
            // Hacen falta las dos excepciones: BCrypt.Net lanza SaltParseException cuando el
            // salt es ilegible, pero ArgumentOutOfRangeException (derivada de ArgumentException)
            // cuando el hash esta truncado y no llega a los 29 caracteres del prefijo.
            return ResultadoVerificacion.Fallida;
        }
    }

    private static ResultadoVerificacion VerificarTextoPlano(string enClaro, string almacenado)
    {
        // Trim() replica el comportamiento de auth_global: la base tiene columnas CHAR
        // con relleno de espacios, y sin el recorte esos usuarios no podrian entrar.
        var coincide = string.Equals(almacenado.Trim(), enClaro, StringComparison.Ordinal);

        // Coincidencia en texto plano siempre pide rehash: es el disparador que migra
        // la fila a BCrypt en el proximo login exitoso.
        return coincide ? ResultadoVerificacion.Valida(requiereRehash: true) : ResultadoVerificacion.Fallida;
    }
}
