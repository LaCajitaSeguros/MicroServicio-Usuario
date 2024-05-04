using Application.Configuration;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Options;
using MimeKit;

namespace Application.Service.ServiceImpl
{
    public class EmailService : IEmailSender
    {
        private readonly SmtpSettings _smtpSettings;
        public EmailService(IOptions<SmtpSettings> smtpSettings) {

            _smtpSettings = smtpSettings.Value;
        }

        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            try
            {
                var message = new MimeMessage();

                message.From.Add(new MailboxAddress(_smtpSettings.SenderName, _smtpSettings.SenderEmail));
                message.To.Add(new MailboxAddress("", email));
                message.Subject = subject;
                message.Body = new TextPart("html")
                {
                    Text = htmlMessage
                };

                using (var client = new SmtpClient())
                {
                    await client.ConnectAsync(_smtpSettings.Server, _smtpSettings.Port, SecureSocketOptions.StartTls);

                    // Autenticación
                    await client.AuthenticateAsync(_smtpSettings.UserName, _smtpSettings.Password);

                    // Envío del mensaje
                    await client.SendAsync(message);

                    // Desconexión del servidor
                    await client.DisconnectAsync(true);
                }
            }
            catch (Exception ex)
            {
                // Manejo de errores
                Console.WriteLine($"Error al enviar correo electrónico: {ex}");
                throw;
            }
        }
    }
}