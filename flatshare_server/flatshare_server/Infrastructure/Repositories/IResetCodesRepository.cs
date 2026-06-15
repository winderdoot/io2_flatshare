namespace flatshare_server.Infrastructure.Repositories;
public interface IResetCodesRepository
{
    public Task SaveNew(Guid userId, string code);
    public Task<bool> CheckValidity(Guid userId, string code);
    public Task InvalidateCodes(Guid userId);
}
