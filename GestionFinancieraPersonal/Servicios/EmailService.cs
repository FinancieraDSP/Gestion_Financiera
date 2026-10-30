using GestionFinancieraPersonal.Models;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;

namespace GestionFinancieraPersonal.Servicios
{
    public class EmailService
    {
        private readonly EmailSettings _settings;

        public EmailService(IOptions<EmailSettings> options)
        {
            _settings = options.Value;
        }

        public async Task EnviarCorreoAsync(
            string destino,
            string asunto,
            string mensajeHtml)
        {
            var email = new MimeMessage();

            email.From.Add(
                new MailboxAddress(
                    _settings.SenderName,
                    _settings.SenderEmail));

            email.To.Add(
                MailboxAddress.Parse(destino));

            email.Subject = asunto;

            email.Body = new BodyBuilder
            {
                HtmlBody = mensajeHtml
            }.ToMessageBody();

            using var smtp = new SmtpClient();

            await smtp.ConnectAsync(
                _settings.SmtpServer,
                _settings.Port,
                SecureSocketOptions.StartTls);

            await smtp.AuthenticateAsync(
                _settings.SenderEmail,
                _settings.Password);

            await smtp.SendAsync(email);

            await smtp.DisconnectAsync(true);
        }

        public async Task EnviarRecuperacionAsync(
        string correo,
        string enlace)
        {
            var html = $@"
            <h2>Recuperación de contraseña</h2>

            <p>Recibimos una solicitud para restablecer tu contraseña.</p>

            <p>
                <a href='{enlace}'>
                    Recuperar contraseña
                </a>
            </p>

            <p>
                Este enlace expirará en 15 minutos.
            </p>

            <p>
                Si no realizaste esta solicitud puedes ignorar este mensaje.
            </p>";

            await EnviarCorreoAsync(
                correo,
                "Recuperación de contraseña",
                html);
        }
    }
}