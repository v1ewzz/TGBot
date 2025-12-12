using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TGBot
{
    [Table("user_requests")]
    public class UserRequest
    {
        public int Id { get; set; }
        public long user_id { get; set; }
        public int count_of_requests { get; set; }
    }

    public class BotDbContext : DbContext
    {
        public DbSet<UserRequest> UserRequests { get; set; }

        public BotDbContext(DbContextOptions<BotDbContext> options)
            : base(options) { }
    }
}
