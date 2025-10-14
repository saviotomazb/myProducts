using System.Net.Mail;
using System.Net;

namespace myProducts.Services
{
    public class EmailService
    {
        public static async Task SendPasswordResetEmailAsync(string recipientEmail, string code)
        {
            string? _smtpHost = Environment.GetEnvironmentVariable("SMTP_HOST");
            string? _smtpPort = Environment.GetEnvironmentVariable("SMTP_PORT");
            string? _smtpUser = Environment.GetEnvironmentVariable("SMTP_USER");
            string? _smtpPass = Environment.GetEnvironmentVariable("SMTP_PASS");

            if (string.IsNullOrWhiteSpace(_smtpHost) ||
                string.IsNullOrWhiteSpace(_smtpPort) ||
                string.IsNullOrWhiteSpace(_smtpUser) ||
                string.IsNullOrWhiteSpace(_smtpPass))
            {
                throw new InvalidOperationException("As variáveis de ambiente SMTP não estão completamente configuradas.");
            }

            if (!int.TryParse(_smtpPort, out int smtpPort))
            {
                throw new InvalidOperationException("A variável SMTP_PORT deve ser um número válido.");
            }

            using var client = new SmtpClient(_smtpHost, smtpPort)
            {
                Credentials = new NetworkCredential(_smtpUser, _smtpPass),
                EnableSsl = true
            };

            var mailMessage = new MailMessage()
            {
                From = new MailAddress(_smtpUser, "MyProducts"),
                Subject = "Código de redefinição de senha - MyProducts",
                Body = $"""
                    <p>Seu código de redefinição de senha é: <strong>{code}</strong></p>
                    <p>O código expira em 15 minutos. Não compartilhe este código com ninguém.</p>
                    """,
                IsBodyHtml = true
            };

            mailMessage.To.Add(recipientEmail);

            try
            {
                await client.SendMailAsync(mailMessage);
                Console.WriteLine($"E-mail de redefinição enviado para: {recipientEmail}");
            }
            catch (SmtpException ex)
            {
                Console.WriteLine($"Erro SMTP: {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao enviar e-mail: {ex.Message}");
                throw;
            }
        }
    }
}