using flatshare_server.Infrastructure.Model.Responses;

namespace flatshare_server.Infrastructure.Model.Exceptions;

public class ServerResponseException : Exception
{
    public ErrorResponse Response { get; private set; }
    public ServerResponseException(ErrorResponse response) 
        : base(response.Error) 
    {
        Response = response;
    }
}
