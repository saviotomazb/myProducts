using System.Net.Mail;
using System.Net;
using Serilog;

namespace myProducts.Services
{
    public class EmailService
    {
        public static async Task SendPasswordResetEmailAsync(string recipientEmail, string code)
        {
            // Carrega as credenciais e configurações de SMTP a partir das variáveis de ambiente (evita expor dados sensíveis no código).
            string? _smtpHost = Environment.GetEnvironmentVariable("SMTP_HOST");
            string? _smtpPort = Environment.GetEnvironmentVariable("SMTP_PORT");
            string? _smtpUser = Environment.GetEnvironmentVariable("SMTP_USER");
            string? _smtpPass = Environment.GetEnvironmentVariable("SMTP_PASS");

            if (string.IsNullOrWhiteSpace(_smtpHost) ||
                string.IsNullOrWhiteSpace(_smtpPort) ||
                string.IsNullOrWhiteSpace(_smtpUser) ||
                string.IsNullOrWhiteSpace(_smtpPass))
            {
                Log.Error("As variáveis de ambiente SMTP não estão completamente configuradas: Host={Host}, Port={Port}, User={User}, PassConfigured={PassConfigured}",
                    _smtpHost, _smtpPort, _smtpUser, !string.IsNullOrWhiteSpace(_smtpPass));
                throw new InvalidOperationException("As variáveis de ambiente SMTP não estão completamente configuradas.");
            }

            if (!int.TryParse(_smtpPort, out int smtpPort))
            {
                Log.Error("A variável SMTP_PORT não é um número válido: {SMTP_PORT}", _smtpPort);
                throw new InvalidOperationException("A variável SMTP_PORT deve ser um número válido.");
            }

            using var client = new SmtpClient(_smtpHost, smtpPort)
            {
                Credentials = new NetworkCredential(_smtpUser, _smtpPass),
                EnableSsl = true
            };


            //Corpo do texto enviado por e-mail
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
                Log.ForContext("SourceContext", "myProducts.Services.EmailService").Information("E-mail de redefinição enviado para: {RecipientEmail}", recipientEmail);
            }
            catch (SmtpException ex)
            {
                Log.Error(ex, "Erro SMTP ao enviar e-mail para: {RecipientEmail}", recipientEmail);
                throw;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Erro ao enviar e-mail: {RecipientEmail}", recipientEmail);
                throw;
            }
        }
    }
}