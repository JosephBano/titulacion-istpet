using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using TitulacionIstpet.Application.Interfaces;

namespace TitulacionIstpet.Infrastructure.Services;

public partial class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    [LoggerMessage(EventId = 2001, Level = LogLevel.Warning, Message = "Intento de envío de correo omitido: dirección de destinatario vacía.")]
    private static partial void LogDestinatarioVacio(ILogger logger);

    [LoggerMessage(EventId = 2002, Level = LogLevel.Information, Message = "[EmailService] Envío de correos deshabilitado por configuración. Correo simulado a {Destinatario}: '{Subject}'")]
    private static partial void LogEnvioDeshabilitado(ILogger logger, string destinatario, string subject);

    [LoggerMessage(EventId = 2003, Level = LogLevel.Information, Message = "Enviando correo a {Destinatario} con asunto '{Subject}' vía {Host}:{Port}...")]
    private static partial void LogEnviando(ILogger logger, string destinatario, string subject, string host, int port);

    [LoggerMessage(EventId = 2004, Level = LogLevel.Information, Message = "Correo enviado exitosamente a {Destinatario}.")]
    private static partial void LogEnviadoExitoso(ILogger logger, string destinatario);

    [LoggerMessage(EventId = 2005, Level = LogLevel.Error, Message = "Error al enviar correo electrónico a {Destinatario}. Detalle: {Detalle}")]
    private static partial void LogErrorEnvio(ILogger logger, string destinatario, string detalle, Exception excepcion);

    public async Task SendEmailAsync(string destinatario, string subject, string htmlBody, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(destinatario))
        {
            LogDestinatarioVacio(_logger);
            return;
        }

        var section = _configuration.GetSection("EmailSettings");
        var habilitado = bool.Parse(section["Habilitado"] ?? "true");
        if (!habilitado)
        {
            LogEnvioDeshabilitado(_logger, destinatario, subject);
            return;
        }

        var host = section["Host"] ?? "smtp.office365.com";
        var portStr = section["Port"] ?? "587";
        var fromEmail = section["FromEmail"] ?? "notificaciones@istpet.edu.ec";
        var password = section["Password"] ?? "K/224393184258ul";
        var senderName = section["SenderName"] ?? "ISTPET - Sistema de Titulación";
        var enableSsl = bool.Parse(section["EnableSsl"] ?? "true");

        int port = int.TryParse(portStr, out var p) ? p : 587;

        using var client = new SmtpClient(host, port)
        {
            UseDefaultCredentials = false,
            Credentials = new NetworkCredential(fromEmail, password),
            EnableSsl = enableSsl
        };

        using var mailMessage = new MailMessage
        {
            From = new MailAddress(fromEmail, senderName),
            Subject = subject,
            Body = htmlBody,
            IsBodyHtml = true
        };

        mailMessage.To.Add(destinatario.Trim());

        LogEnviando(_logger, destinatario, subject, host, port);

        try
        {
            await client.SendMailAsync(mailMessage, cancellationToken);
            LogEnviadoExitoso(_logger, destinatario);
        }
        catch (Exception ex)
        {
            LogErrorEnvio(_logger, destinatario, ex.Message, ex);
            // No propagamos la excepción para evitar que un fallo de red o SMTP interrumpa la transacción del usuario
        }
    }

    public async Task NotificarPostulacionRegistradaAsync(
        string destinatario,
        string nombreEstudiante,
        string carrera,
        string modalidad,
        int idPostulacion,
        CancellationToken cancellationToken = default)
    {
        string subject = $"Confirmación de Postulación al Proceso de Titulación #{idPostulacion} - ISTPET";
        string contenido = $@"
            <p>Estimado/a <strong>{nombreEstudiante}</strong>,</p>
            <p>Le notificamos que su solicitud formal de <strong>postulación al proceso de titulación</strong> ha sido registrada exitosamente en el sistema institucional.</p>
            
            <div style='background: #f8fafc; border-left: 4px solid #0d1b4c; padding: 14px 18px; margin: 18px 0; border-radius: 4px;'>
                <table style='width: 100%; border-collapse: collapse; font-size: 14px;'>
                    <tr><td style='padding: 6px 0; color: #64748b; width: 140px;'><strong>N° Expediente:</strong></td><td style='padding: 6px 0; color: #0d1b4c;'><strong>#{idPostulacion}</strong></td></tr>
                    <tr><td style='padding: 6px 0; color: #64748b;'><strong>Carrera:</strong></td><td style='padding: 6px 0;'>{carrera}</td></tr>
                    <tr><td style='padding: 6px 0; color: #64748b;'><strong>Modalidad:</strong></td><td style='padding: 6px 0;'><strong>{modalidad}</strong></td></tr>
                    <tr><td style='padding: 6px 0; color: #64748b;'><strong>Estado Actual:</strong></td><td style='padding: 6px 0;'><span style='background: #fef3c7; color: #92400e; padding: 3px 8px; border-radius: 3px; font-weight: bold;'>EN REVISIÓN</span></td></tr>
                </table>
            </div>

            <p style='color: #334155;'><strong>📌 ¿Cuál es el siguiente paso?</strong></p>
            <p style='color: #475569;'>Su postulación se encuentra en la fase de evaluación y verificación académica por parte de la <strong>Coordinación de Carrera</strong>. Una vez que la Coordinación emita el dictamen oficial de aprobación, usted quedará formalmente admitido en esta modalidad para continuar cumpliendo los requisitos a lo largo del período.</p>
        ";

        string html = ConstruirPlantillaBase("SOLICITUD DE POSTULACIÓN REGISTRADA", "EN REVISIÓN", "#b45309", contenido);
        await SendEmailAsync(destinatario, subject, html, cancellationToken);
    }

    public async Task NotificarDictamenPostulacionAsync(
        string destinatario,
        string nombreEstudiante,
        string carrera,
        string modalidad,
        int idPostulacion,
        string decision,
        string? observaciones,
        CancellationToken cancellationToken = default)
    {
        string decUpper = (decision ?? string.Empty).ToUpperInvariant();
        bool esAprobado = decUpper.Contains("APROB") || decUpper.Contains("ACEPT");
        bool esRechazado = decUpper.Contains("RECHAZ") || decUpper.Contains("NEGAD");

        string subject;
        string badgeTexto;
        string badgeColor;
        string contenido;

        if (esAprobado)
        {
            subject = $"¡Postulación Aprobada! - Admisión Oficial a Modalidad de Titulación #{idPostulacion} - ISTPET";
            badgeTexto = "POSTULACIÓN APROBADA";
            badgeColor = "#15803d";
            contenido = $@"
                <p>Estimado/a <strong>{nombreEstudiante}</strong>,</p>
                <p>Nos complace informarle que la Coordinación de Carrera ha <strong>APROBADO</strong> su postulación al proceso de titulación para la carrera de <strong>{carrera}</strong>.</p>

                <div style='background: #f0fdf4; border-left: 4px solid #16a34a; padding: 14px 18px; margin: 18px 0; border-radius: 4px;'>
                    <table style='width: 100%; border-collapse: collapse; font-size: 14px;'>
                        <tr><td style='padding: 6px 0; color: #64748b; width: 140px;'><strong>N° Expediente:</strong></td><td style='padding: 6px 0; color: #166534;'><strong>#{idPostulacion}</strong></td></tr>
                        <tr><td style='padding: 6px 0; color: #64748b;'><strong>Modalidad Asignada:</strong></td><td style='padding: 6px 0; color: #166534;'><strong>{modalidad}</strong></td></tr>
                        <tr><td style='padding: 6px 0; color: #64748b;'><strong>Dictamen:</strong></td><td style='padding: 6px 0;'><span style='background: #dcfce7; color: #15803d; padding: 3px 8px; border-radius: 3px; font-weight: bold;'>APROBADO</span></td></tr>
                    </table>
                </div>

                <p style='color: #1e293b;'><strong>🎓 Incorporación Oficial y Desarrollo Académico:</strong></p>
                <p style='color: #475569;'>Usted se encuentra formalmente admitido en la modalidad de <strong>{modalidad}</strong>. A lo largo del período académico de su cohorte deberá ir completando y cargando sus requisitos para que los docentes tutores y revisores los evalúen oportunamente hasta su graduación.</p>
            ";
        }
        else if (esRechazado)
        {
            subject = $"Resolución sobre su Postulación de Titulación #{idPostulacion} - ISTPET";
            badgeTexto = "SOLICITUD RECHAZADA";
            badgeColor = "#b91c1c";
            contenido = $@"
                <p>Estimado/a <strong>{nombreEstudiante}</strong>,</p>
                <p>Le comunicamos que la Coordinación de Carrera ha concluido la revisión de su postulación a la modalidad de <strong>{modalidad}</strong> en el expediente <strong>#{idPostulacion}</strong> con dictamen de <strong>RECHAZO</strong>.</p>

                <div style='background: #fef2f2; border-left: 4px solid #dc2626; padding: 14px 18px; margin: 18px 0; border-radius: 4px;'>
                    <table style='width: 100%; border-collapse: collapse; font-size: 14px;'>
                        <tr><td style='padding: 6px 0; color: #64748b; width: 140px;'><strong>N° Expediente:</strong></td><td style='padding: 6px 0; color: #991b1b;'><strong>#{idPostulacion}</strong></td></tr>
                        <tr><td style='padding: 6px 0; color: #64748b;'><strong>Modalidad:</strong></td><td style='padding: 6px 0;'>{modalidad}</td></tr>
                        <tr><td style='padding: 6px 0; color: #64748b;'><strong>Resolución:</strong></td><td style='padding: 6px 0;'><span style='background: #fee2e2; color: #b91c1c; padding: 3px 8px; border-radius: 3px; font-weight: bold;'>RECHAZADO</span></td></tr>
                        {(!string.IsNullOrWhiteSpace(observaciones) ? $"<tr><td style='padding: 6px 0; color: #64748b;'><strong>Motivo del Rechazo:</strong></td><td style='padding: 6px 0; color: #7f1d1d;'>{observaciones}</td></tr>" : "")}
                    </table>
                </div>

                <p style='color: #1e293b;'><strong>🔄 ¿Cómo continuar su proceso?</strong></p>
                <p style='color: #475569;'>Mientras el período de convocatoria de titulación se encuentre dentro de los plazos vigentes, usted puede ingresar al portal institucional y <strong>cambiar de modalidad de titulación</strong> para postularse a otra de las modalidades habilitadas para su carrera.</p>
            ";
        }
        else
        {
            subject = $"Observaciones en su Postulación de Titulación #{idPostulacion} - ISTPET";
            badgeTexto = "SOLICITUD OBSERVADA";
            badgeColor = "#b45309";
            contenido = $@"
                <p>Estimado/a <strong>{nombreEstudiante}</strong>,</p>
                <p>Le informamos que la Coordinación de Carrera ha registrado observaciones en su expediente <strong>#{idPostulacion}</strong> para la modalidad de <strong>{modalidad}</strong>.</p>

                <div style='background: #fffbeb; border-left: 4px solid #f59e0b; padding: 14px 18px; margin: 18px 0; border-radius: 4px;'>
                    <p style='margin: 0; color: #92400e;'><strong>Detalle de Observaciones:</strong></p>
                    <p style='margin: 8px 0 0; color: #78350f;'>{observaciones ?? "Por favor revise los requisitos en el portal para subsanar los documentos pendientes."}</p>
                </div>

                <p style='color: #475569;'>Ingrese al sistema de titulación para revisar los detalles y actualizar la documentación requerida.</p>
            ";
        }

        string html = ConstruirPlantillaBase("RESOLUCIÓN OFICIAL DE POSTULACIÓN", badgeTexto, badgeColor, contenido);
        await SendEmailAsync(destinatario, subject, html, cancellationToken);
    }

    public async Task NotificarCambioModalidadAsync(
        string destinatario,
        string nombreEstudiante,
        string carrera,
        string nuevaModalidad,
        int idPostulacion,
        CancellationToken cancellationToken = default)
    {
        string subject = $"Actualización de Modalidad de Postulación #{idPostulacion} - ISTPET";
        string contenido = $@"
            <p>Estimado/a <strong>{nombreEstudiante}</strong>,</p>
            <p>Le confirmamos que se ha registrado con éxito el <strong>cambio de modalidad de postulación</strong> en su expediente <strong>#{idPostulacion}</strong>.</p>

            <div style='background: #f8fafc; border-left: 4px solid #0d1b4c; padding: 14px 18px; margin: 18px 0; border-radius: 4px;'>
                <table style='width: 100%; border-collapse: collapse; font-size: 14px;'>
                    <tr><td style='padding: 6px 0; color: #64748b; width: 150px;'><strong>N° Expediente:</strong></td><td style='padding: 6px 0; color: #0d1b4c;'><strong>#{idPostulacion}</strong></td></tr>
                    <tr><td style='padding: 6px 0; color: #64748b;'><strong>Carrera:</strong></td><td style='padding: 6px 0;'>{carrera}</td></tr>
                    <tr><td style='padding: 6px 0; color: #64748b;'><strong>Nueva Modalidad:</strong></td><td style='padding: 6px 0; color: #0d1b4c;'><strong>{nuevaModalidad}</strong></td></tr>
                    <tr><td style='padding: 6px 0; color: #64748b;'><strong>Nuevo Estado:</strong></td><td style='padding: 6px 0;'><span style='background: #fef3c7; color: #92400e; padding: 3px 8px; border-radius: 3px; font-weight: bold;'>EN REVISIÓN</span></td></tr>
                </table>
            </div>

            <p style='color: #334155;'><strong>📌 Siguiente Paso:</strong></p>
            <p style='color: #475569;'>Su postulación ha reingresado a la bandeja de la <strong>Coordinación de Carrera</strong> para la emisión de un nuevo dictamen de admisión. Los documentos comunes a ambas modalidades se han conservado automáticamente en su expediente.</p>
        ";

        string html = ConstruirPlantillaBase("CAMBIO DE MODALIDAD REGISTRADO", "EN EVALUACIÓN", "#0d1b4c", contenido);
        await SendEmailAsync(destinatario, subject, html, cancellationToken);
    }

    private static string ConstruirPlantillaBase(string subtituloHeader, string badgeTexto, string badgeColor, string cuerpoContenido)
    {
        return $@"
<!DOCTYPE html>
<html lang='es'>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>Notificación ISTPET</title>
</head>
<body style='margin: 0; padding: 0; background-color: #f1f5f9; font-family: -apple-system, BlinkMacSystemFont, ""Segoe UI"", Roboto, Helvetica, Arial, sans-serif; color: #1e293b;'>
    <table role='presentation' width='100%' cellspacing='0' cellpadding='0' style='background-color: #f1f5f9; padding: 30px 10px;'>
        <tr>
            <td align='center'>
                <table role='presentation' width='100%' style='max-width: 600px; background-color: #ffffff; border-radius: 8px; overflow: hidden; box-shadow: 0 4px 6px -1px rgba(0, 0, 0, 0.05), 0 2px 4px -1px rgba(0, 0, 0, 0.03); border: 1px solid #e2e8f0;'>
                    <!-- Header Oficial ISTPET -->
                    <tr>
                        <td style='background: linear-gradient(135deg, #0d1b4c 0%, #1e293b 100%); padding: 28px 30px; text-align: left; border-bottom: 3px solid #c59b27;'>
                            <div style='font-size: 11px; font-weight bold; color: #c59b27; text-transform: uppercase; letter-spacing: 0.1em; margin-bottom: 4px;'>ISTPET · SISTEMA INTEGRAL DE TITULACIÓN</div>
                            <h1 style='margin: 0; font-size: 20px; font-weight: 700; color: #ffffff;'>Instituto Superior Tecnológico Mayor Pedro Traversari</h1>
                            <div style='font-size: 13px; color: #cbd5e1; margin-top: 6px;'>{subtituloHeader}</div>
                        </td>
                    </tr>

                    <!-- Badge Estado -->
                    <tr>
                        <td style='padding: 24px 30px 10px;'>
                            <div style='display: inline-block; background-color: {badgeColor}; color: #ffffff; font-size: 11px; font-weight: bold; letter-spacing: 0.05em; text-transform: uppercase; padding: 5px 12px; border-radius: 20px;'>
                                {badgeTexto}
                            </div>
                        </td>
                    </tr>

                    <!-- Contenido Central -->
                    <tr>
                        <td style='padding: 10px 30px 24px; font-size: 14px; line-height: 1.6; color: #334155;'>
                            {cuerpoContenido}
                        </td>
                    </tr>

                    <!-- Footer Oficial -->
                    <tr>
                        <td style='background-color: #f8fafc; border-top: 1px solid #e2e8f0; padding: 20px 30px; text-align: center; font-size: 12px; color: #64748b;'>
                            <p style='margin: 0 0 6px;'><strong>Instituto Superior Tecnológico Mayor Pedro Traversari (ISTPET)</strong></p>
                            <p style='margin: 0 0 10px;'>Dirección de Titulación e Investigación Académica</p>
                            <p style='margin: 0; font-size: 11px; color: #94a3b8;'>Este correo es generado de manera automática por el Sistema de Titulación ISTPET. Por favor, no responda a este mensaje.</p>
                        </td>
                    </tr>
                </table>
            </td>
        </tr>
    </table>
</body>
</html>";
    }
}
