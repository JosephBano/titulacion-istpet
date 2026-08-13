namespace TitulacionIstpet.Domain.Common;

/// <summary>
/// Raiz de toda entidad persistida. La identidad es <see cref="Id"/>, no la referencia
/// del objeto: dos instancias cargadas por separado del mismo registro son iguales.
/// </summary>
public abstract class EntidadBase
{
    public int Id { get; protected set; }

    public DateTime CreadoEn { get; set; }
    public string? CreadoPor { get; set; }
    public DateTime? ModificadoEn { get; set; }
    public string? ModificadoPor { get; set; }

    public override bool Equals(object? obj) =>
        obj is EntidadBase otra && otra.GetType() == GetType() && Id != 0 && Id == otra.Id;

    public override int GetHashCode() => HashCode.Combine(GetType(), Id);
}
