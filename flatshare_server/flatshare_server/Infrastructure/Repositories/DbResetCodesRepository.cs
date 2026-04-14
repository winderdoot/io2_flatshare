
using flatshare_server.Infrastructure.Model;
using Microsoft.EntityFrameworkCore;

namespace flatshare_server.Infrastructure.Repositories;
public class DbResetCodesRepository : IResetCodesRepository
{
    private readonly FlatshareDbContext _context;
    public DbResetCodesRepository(FlatshareDbContext dbcontext)
    {
        _context = dbcontext;
    }

    public async Task<bool> CheckValidity(Guid userId, string code)
    {
        var entry = await _context.PasswordResetEntries
            .Where(e => e.UserId == userId && e.IsValid && e.ResetCode == code)
            .FirstOrDefaultAsync();

        if (entry == null || entry.ExpiresAt < DateTime.UtcNow)
        {
            return false;
        }
        return true;
    }

    public async Task InvalidateCodes(Guid userId)
    {
        await _context.PasswordResetEntries
            .Where(e => e.UserId == userId && e.IsValid)
            .ExecuteUpdateAsync(set => set.SetProperty(s => s.IsValid, false));

        await _context.SaveChangesAsync();
    }

    public async Task SaveNew(Guid userId, string code)
    {
        var entry = new PasswordResetEntry { UserId = userId, ResetCode = code };
        await _context.PasswordResetEntries.AddAsync(entry);

        await _context.SaveChangesAsync();
    }
}
