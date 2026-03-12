using flatshare_server.Infrastructure.Model.User;
using Microsoft.EntityFrameworkCore;

namespace flatshare_server.Infrastructure.Repositories;

public class DbUserRepository : IUserRepository
{
    private readonly FlatshareDbContext _context;
    public DbUserRepository(FlatshareDbContext dbcontext)
    {
        _context = dbcontext;
    }

    public async Task SaveNew(User user)
    {
        var alreadyExists = await _context.Users
            .AnyAsync(u => u.Id == user.Id || u.Email == user.Email);
        if (alreadyExists)
        {
            throw new ArgumentException("User with this email already exists!");
        }

        _context.Users.Add(user);
        await _context.SaveChangesAsync();
    }
}
