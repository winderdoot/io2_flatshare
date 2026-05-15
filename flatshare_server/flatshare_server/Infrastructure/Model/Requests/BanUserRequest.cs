using flatshare_server.Infrastructure.Model.Users;

namespace flatshare_server.Infrastructure.Model.Requests;

public record class BanUserRequest(string Reason);
