using Microsoft.EntityFrameworkCore;
using TGBot.Abstractions;
using TGBot.Data;

namespace TGBot.Services;

public sealed class UserRequestsService : IRequestLimiter
{
    private const int RequestLimit = 3;

    private readonly string _connectionString;

    public UserRequestsService(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task EnsureInitializedAsync(CancellationToken cancellationToken = default)
    {
        await using BotDbContext context = CreateContext();
        await context.Database.EnsureCreatedAsync(cancellationToken);
    }

    public async Task<bool> CanMakeRequestAsync(long userId, CancellationToken cancellationToken = default)
    {
        await using BotDbContext context = CreateContext();

        UserRequest? user = await context.UserRequests
            .FirstOrDefaultAsync(u => u.UserId == userId, cancellationToken);

        return user is null || user.CountOfRequests < RequestLimit;
    }

    public async Task<bool> TryConsumeRequestAsync(long userId, CancellationToken cancellationToken = default)
    {
        await using BotDbContext context = CreateContext();

        int rows = await context.Database.ExecuteSqlInterpolatedAsync(
            $@"UPDATE user_requests
               SET count_of_requests = count_of_requests + 1
               WHERE user_id = {userId} AND count_of_requests < {RequestLimit}",
            cancellationToken);

        if (rows > 0)
            return true;

        bool exists = await context.UserRequests.AnyAsync(u => u.UserId == userId, cancellationToken);
        if (exists)
            return false;

        context.UserRequests.Add(new UserRequest { UserId = userId, CountOfRequests = 1 });
        try
        {
            await context.SaveChangesAsync(cancellationToken);
            return true;
        }
        catch (DbUpdateException)
        {
            return await TryConsumeRequestAsync(userId, cancellationToken);
        }
    }

    private BotDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<BotDbContext>()
            .UseSqlite(_connectionString)
            .Options;
        return new BotDbContext(options);
    }
}