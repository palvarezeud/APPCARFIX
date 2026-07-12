using CarFix.Dominio.Interfaces;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MimeKit;

namespace CarFix.Infraestructura.Correo;

public class ServicioEnvioCorreoSmtp : IServicioEnvioCorreo
{
    private readonly IConfiguration _config;
    private readonly ILogger<ServicioEnvioCorreoSmtp> _logger;

    public ServicioEnvioCorreoSmtp(IConfiguration config, ILogger<ServicioEnvioCorreoSmtp> logger)
    {
        _config = config;
        _logger = logger;
    }

    public async Task<ResultadoEnvioCorreo> EnviarAsync(
        string destinatario, string asunto, string cuerpoHtml,
        byte[] adjuntoBytes, string nombreAdjunto, CancellationToken ct = default)
    {
        var host              = _config["Smtp:Host"];
        var usuarioRemitente  = _config["Smtp:UsuarioRemitente"];
        var contrasenna       = _config["Smtp:Contrasenna"];

        if (string.IsNullOrWhiteSpace(host) || string.IsNullOrWhiteSpace(usuarioRemitente) || string.IsNullOrWhiteSpace(contrasenna))
        {
            _logger.LogError("Falta configurar Smtp:Host, Smtp:UsuarioRemitente o Smtp:Contrasenna.");
            return ResultadoEnvioCorreo.Fallo(
                "El servidor de correo no esta configurado. Contacte al administrador del sistema.");
        }

        var puerto        = int.TryParse(_config["Smtp:Puerto"], out var p) ? p : 587;
        var usarSsl        = bool.TryParse(_config["Smtp:UsarSsl"], out var s) ? s : true;
        var nombreRemitente = _config["Smtp:NombreRemitente"] ?? "CAR FIX";

        try
        {
            var mensaje = new MimeMessage();
            mensaje.From.Add(new MailboxAddress(nombreRemitente, usuarioRemitente));
            mensaje.To.Add(MailboxAddress.Parse(destinatario));
            mensaje.Subject = asunto;

            var cuerpo = new BodyBuilder { HtmlBody = cuerpoHtml };
            cuerpo.Attachments.Add(nombreAdjunto, adjuntoBytes, new ContentType("application", "pdf"));
            mensaje.Body = cuerpo.ToMessageBody();

            using var cliente = new SmtpClient();
            await cliente.ConnectAsync(host, puerto, usarSsl ? SecureSocketOptions.StartTls : SecureSocketOptions.None, ct);
            await cliente.AuthenticateAsync(usuarioRemitente, contrasenna, ct);
            await cliente.SendAsync(mensaje, ct);
            await cliente.DisconnectAsync(true, ct);

            return ResultadoEnvioCorreo.Exito();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al enviar correo a {Destinatario}.", destinatario);
            return ResultadoEnvioCorreo.Fallo(
                "No se pudo enviar el correo en este momento. Intente de nuevo mas tarde.");
        }
    }
}
