using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TGBot.Data;

[Table("user_requests")]
public sealed class UserRequest
{
    public int Id { get; set; }

    [Column("user_id")]
    public long UserId { get; set; }

    [Column("count_of_requests")]
    public int CountOfRequests { get; set; }
}

public sealed class BotDbContext : DbContext
{
    public DbSet<UserRequest> UserRequests { get; set; } = null!;

    public BotDbContext(DbContextOptions<BotDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UserRequest>()
            .HasIndex(u => u.UserId)
            .IsUnique();
    }
}