using flatshare_server.Infrastructure.Configuration;
using flatshare_server.Infrastructure.Model.Responses;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;

namespace flatshare_server.Infrastructure.Services
{
    public class EmailService
    {
        private readonly EmailOptions _options;

        public EmailService(IOptions<EmailOptions> options)
        {
            _options = options.Value;
        }

        public async Task SendEmailHtmlAsync(UserDTO user, string subject, string body)
        {
            var msg = new MimeMessage();
            msg.From.Add(new MailboxAddress(_options.AppName, _options.EmailAddress));
            msg.To.Add(new MailboxAddress($"{user.FirstName} {user.LastName}", user.Email));
            msg.Subject = subject;
            msg.Body = new TextPart(MimeKit.Text.TextFormat.Html) { Text = body };

            using var smtp = new SmtpClient();

            await smtp.ConnectAsync(_options.Host, _options.Port, SecureSocketOptions.StartTls);
            await smtp.AuthenticateAsync(_options.EmailAddress, _options.AppPassword);

            await smtp.SendAsync(msg);
            await smtp.DisconnectAsync(true);
        }
    }
}
