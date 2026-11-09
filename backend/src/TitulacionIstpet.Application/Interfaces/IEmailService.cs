namespace TitulacionIstpet.Application.Interfaces;

public interface IEmailService
{
    Task SendEmailAsync(string destinatario, string subject, string htmlBody, CancellationToken cancellationToken = default);

    Task NotificarPostulacionRegistradaAsync(
        string destinatario,
        string nombreEstudiante,
        string carrera,
        string modalidad,
        int idPostulacion,
        CancellationToken cancellationToken = default);

    Task NotificarDictamenPostulacionAsync(
        string destinatario,
        string nombreEstudiante,
        string carrera,
        string modalidad,
        int idPostulacion,
        string decision,
        string? observaciones,
        CancellationToken cancellationToken = default);

    Task NotificarCambioModalidadAsync(
        string destinatario,
        string nombreEstudiante,
        string carrera,
        string nuevaModalidad,
        int idPostulacion,
        CancellationToken cancellationToken = default);
}
