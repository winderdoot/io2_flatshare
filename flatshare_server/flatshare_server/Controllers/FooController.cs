using flatshare_server.Infrastructure.Repositories;
using Microsoft.AspNetCore.Mvc;
using flatshare_server.Infrastructure.Model;

namespace flatshare_server.Controllers;

/* Test controller */

[ApiController]
[Route("[controller]")]
public class FooController : ControllerBase
{
    private readonly ILogger<FooController> _logger;

    public FooController(ILogger<FooController> logger)
    {
        _logger = logger;
    }

    [HttpGet("foo")]
    public async Task<ActionResult<string>> Get(FlatshareDbContext dbcontext)
    {
        string bar = "Gugu gaga";

        dbcontext.Foos.Add(new Foo { Bar = bar });
        await dbcontext.SaveChangesAsync();

        return $"Successfuly added '{bar}' to the database!";
    }
}
