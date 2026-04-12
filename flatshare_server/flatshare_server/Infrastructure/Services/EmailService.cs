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
        private readonly string _appName;
        private readonly string _hostAddress;
        private readonly int _port;
        private readonly string _emailAddress;
        private readonly string _password;

        public EmailService(IOptions<EmailOptions> options)
        {
            var opt = options.Value;

            _appName = opt.AppName;
            _hostAddress = opt.Host;
            _port = opt.Port;
            _emailAddress = opt.EmailAddress;
            _password = opt.AppPassword;
        }

        public async Task SendEmailHtmlAsync(UserDTO user, string subject, string body)
        {
            var msg = new MimeMessage();
            msg.From.Add(new MailboxAddress(_appName, _emailAddress));
            msg.To.Add(new MailboxAddress($"{user.FirstName} {user.LastName}", user.Email));
            msg.Subject = subject;
            msg.Body = new TextPart(MimeKit.Text.TextFormat.Html) { Text = body };

            using var smtp = new SmtpClient();

            await smtp.ConnectAsync(_hostAddress, _port, SecureSocketOptions.StartTls);
            await smtp.AuthenticateAsync(_emailAddress, _password);

            await smtp.SendAsync(msg);
            await smtp.DisconnectAsync(true);
        }
    }
}
