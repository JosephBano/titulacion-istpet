using Titan.Domain.Common;
using Titan.Domain.Enums;
using Titan.Domain.Exceptions;

namespace Titan.Domain.Entities;

/// <summary>
/// Entidad de ejemplo del modelo inicial de titulación.
/// </summary>
public class Estudiante : EntidadBase
{
    public string Cedula { get; private set; } = null!;
    public string Nombres { get; private set; } = null!;
    public string Apellidos { get; private set; } = null!;
    public string CorreoInstitucional { get; private set; } = null!;
    public EstadoTitulacion Estado { get; private set; } = EstadoTitulacion.Borrador;

    private Estudiante() { }

    public Estudiante(string cedula, string nombres, string apellidos, string correoInstitucional)
    {
        if (string.IsNullOrWhiteSpace(cedula))
        {
            throw new DominioException("La cedula es obligatoria.");
        }

        if (string.IsNullOrWhiteSpace(correoInstitucional))
        {
            throw new DominioException("El correo institucional es obligatorio.");
        }

        Cedula = cedula.Trim();
        Nombres = nombres.Trim();
        Apellidos = apellidos.Trim();
        CorreoInstitucional = correoInstitucional.Trim().ToLowerInvariant();
    }

    public string NombreCompleto => $"{Apellidos} {Nombres}";

    public void AvanzarA(EstadoTitulacion nuevo)
    {
        if (Estado == EstadoTitulacion.Titulado)
        {
            throw new DominioException("Un estudiante titulado no puede cambiar de estado.");
        }

        Estado = nuevo;
    }
}
