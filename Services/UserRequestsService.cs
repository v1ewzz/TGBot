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
        using BotDbContext context = CreateContext();
        await context.Database.ExecuteSqlRawAsync(InitializeSql, cancellationToken);
    }

    public async Task<bool> CanMakeRequestAsync(long userId, CancellationToken cancellationToken = default)
    {
        using BotDbContext context = CreateContext();

        UserRequest? user = await context.UserRequests
            .FirstOrDefaultAsync(u => u.UserId == userId, cancellationToken);

        return user is null || user.CountOfRequests < RequestLimit;
    }

    public async Task<bool> TryConsumeRequestAsync(long userId, CancellationToken cancellationToken = default)
    {
        using BotDbContext context = CreateContext();

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
            .UseSqlServer(_connectionString)
            .Options;
        return new BotDbContext(options);
    }

    private const string InitializeSql = """
        IF OBJECT_ID('dbo.user_requests', 'U') IS NULL
        BEGIN
            CREATE TABLE dbo.user_requests (
                Id INT IDENTITY PRIMARY KEY,
                user_id BIGINT NOT NULL,
                count_of_requests INT NOT NULL DEFAULT 0
            );
        END;

        IF NOT EXISTS (
            SELECT 1 FROM sys.indexes
            WHERE name = 'UX_user_requests_user_id'
              AND object_id = OBJECT_ID('dbo.user_requests')
        )
        BEGIN
            CREATE UNIQUE INDEX UX_user_requests_user_id ON dbo.user_requests(user_id);
        END;
        """;
}