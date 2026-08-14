namespace TitulacionIstpet.Application.Auth;

/// <summary>
/// Puerto de verificacion de contrasenias. La implementacion vive en Infrastructure
/// porque depende de BCrypt; la capa de aplicacion solo conoce esta interfaz.
/// </summary>
public interface IVerificadorCredenciales
{
    /// <summary>
    /// Compara la contrasenia en claro con lo que hay almacenado, sea un hash BCrypt
    /// o una contrasenia legacy en texto plano.
    /// </summary>
    ResultadoVerificacion Verificar(string? contraseniaEnClaro, string? almacenado);

    /// <summary>Genera un hash BCrypt con el mismo work factor que el resto del ecosistema.</summary>
    string Hashear(string contraseniaEnClaro);

    /// <summary>Un valor almacenado es un hash BCrypt si empieza por el prefijo <c>$2</c>.</summary>
    bool EsHash(string? almacenado);
}

/// <param name="EsValida">La contrasenia coincide.</param>
/// <param name="RequiereRehash">
/// Coincidio, pero estaba en texto plano o con un work factor obsoleto. Quien llama
/// deberia regrabarla hasheada. Nunca es true si <see cref="EsValida"/> es false.
/// </param>
public readonly record struct ResultadoVerificacion(bool EsValida, bool RequiereRehash)
{
    public static readonly ResultadoVerificacion Fallida = new(false, false);

    public static ResultadoVerificacion Valida(bool requiereRehash = false) =>
        new(true, requiereRehash);
}
