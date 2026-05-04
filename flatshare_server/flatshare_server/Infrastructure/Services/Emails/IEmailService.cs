using flatshare_server.Infrastructure.Model.Responses;

namespace flatshare_server.Infrastructure.Services.Emails;

public interface IEmailService
{
    public Task SendEmailHtmlAsync(UserDTO user, string subject, string body);
}
