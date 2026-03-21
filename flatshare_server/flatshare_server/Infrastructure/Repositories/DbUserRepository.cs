using flatshare_server.Infrastructure.Model.Responses;
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
            throw ErrorResponse.Generate("User with this email already exists!");
        }

        _context.Users.Add(user);
        await _context.SaveChangesAsync();
    }
    public async Task<User> GetById(Guid id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user is null)
        {
            throw ErrorResponse.Generate(
                $"User with Id: '{id}' doesn't exist",
                StatusCodes.Status404NotFound
            );
        }

        return user;
    }

    public async Task<User?> GetByEmail(string email)
    {
        return await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
    }
}
