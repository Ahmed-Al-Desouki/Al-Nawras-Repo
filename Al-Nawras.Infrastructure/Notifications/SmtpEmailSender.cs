using System.Net;
using System.Net.Mail;
using Al_Nawras.Application.Common.Notifications;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Al_Nawras.Infrastructure.Notifications
{
    public class SmtpEmailSender : IEmailSender
    {
        private readonly EmailOptions _options;
        private readonly ILogger<SmtpEmailSender> _logger;

        public SmtpEmailSender(
            IOptions<EmailOptions> options,
            ILogger<SmtpEmailSender> logger)
        {
            _options = options.Value;
            _logger = logger;
        }

        public async Task SendAsync(
            string toEmail,
            string subject,
            string htmlBody,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(_options.SmtpHost) ||
                string.IsNullOrWhiteSpace(_options.FromEmail))
            {
                _logger.LogWarning(
                    "Email settings are incomplete. Skipping email to {Email}.", toEmail);
                return;
            }

            using var message = new MailMessage
            {
                From = new MailAddress(_options.FromEmail, _options.FromName ?? "Al-Nawras"),
                Subject = subject,
                Body = htmlBody,
                IsBodyHtml = true
            };

            message.To.Add(toEmail);

            using var client = new SmtpClient(_options.SmtpHost, _options.Port)
            {
                EnableSsl = _options.EnableSsl
            };

            if (!string.IsNullOrWhiteSpace(_options.Username))
            {
                client.Credentials = new NetworkCredential(
                    _options.Username,
                    _options.Password);
            }

            cancellationToken.ThrowIfCancellationRequested();
            await client.SendMailAsync(message, cancellationToken);
        }
    }
}
