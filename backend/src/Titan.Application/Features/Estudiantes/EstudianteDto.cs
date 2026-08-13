using Titan.Domain.Entities;
using Titan.Domain.Enums;

namespace Titan.Application.Features.Estudiantes;

public record EstudianteDto(
    int Id,
    string Cedula,
    string Nombres,
    string Apellidos,
    string CorreoInstitucional,
    EstadoTitulacion Estado)
{
    public static EstudianteDto Desde(Estudiante e) =>
        new(e.Id, e.Cedula, e.Nombres, e.Apellidos, e.CorreoInstitucional, e.Estado);
}
