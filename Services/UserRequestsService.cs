using Microsoft.EntityFrameworkCore;
using TGBot.Abstractions;
using TGBot.Data;

namespace TGBot.Services;

public sealed class UserRequestsService : IRequestLimiter
{
    private const int RequestLimit = 15;

    private readonly string _connectionString;

    public UserRequestsService(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task EnsureInitializedAsync(CancellationToken cancellationToken = default)
    {
        await using BotDbContext context = CreateContext();
        await context.Database.EnsureCreatedAsync(cancellationToken);
        await MigrateAsync(context, cancellationToken);
    }

    public async Task<bool> CanMakeRequestAsync(long userId, CancellationToken cancellationToken = default)
    {
        await using BotDbContext context = CreateContext();

        UserRequest? user = await context.UserRequests
            .FirstOrDefaultAsync(u => u.UserId == userId, cancellationToken);

        if (user is null || user.LastRequestDate != Today)
            return true;

        return user.CountOfRequests < RequestLimit;
    }

    public async Task<bool> TryConsumeRequestAsync(long userId, CancellationToken cancellationToken = default)
    {
        await using BotDbContext context = CreateContext();

        int rows = await context.Database.ExecuteSqlInterpolatedAsync(
            $@"UPDATE user_requests
               SET count_of_requests = CASE
                       WHEN last_request_date IS NULL OR last_request_date <> {Today} THEN 1
                       ELSE count_of_requests + 1
                   END,
                   last_request_date = {Today}
               WHERE user_id = {userId}
                 AND (last_request_date IS NULL OR last_request_date <> {Today} OR count_of_requests < {RequestLimit})",
            cancellationToken);

        if (rows > 0)
            return true;

        bool exists = await context.UserRequests.AnyAsync(u => u.UserId == userId, cancellationToken);
        if (exists)
            return false;

        context.UserRequests.Add(new UserRequest
        {
            UserId = userId,
            CountOfRequests = 1,
            LastRequestDate = Today
        });
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

    private async Task MigrateAsync(BotDbContext context, CancellationToken cancellationToken)
    {
        var columns = await context.Database
            .SqlQueryRaw<string>("SELECT name FROM pragma_table_info('user_requests')")
            .ToListAsync(cancellationToken);

        if (!columns.Contains("last_request_date"))
        {
            await context.Database.ExecuteSqlRawAsync(
                "ALTER TABLE user_requests ADD COLUMN last_request_date TEXT", cancellationToken);
        }
    }

    private BotDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<BotDbContext>()
            .UseSqlite(_connectionString)
            .Options;
        return new BotDbContext(options);
    }

    private static string Today => DateTime.UtcNow.ToString("yyyy-MM-dd");
}