using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace TGBot
{
    internal class UserRequestsService
    {
        //private readonly BotDbContext _context;
        private readonly string _connectionString;
        public UserRequestsService()
        {
            _connectionString = "Server=LAPTOP-O96BRCVR\\SQLEXPRESS;" +
                "Database=TGbot;" +
                "Trusted_Connection=true;" +
                "TrustServerCertificate=true;";
            //var options = new DbContextOptionsBuilder<BotDbContext>()
            //    .UseSqlServer(connectionString)
            //    .Options;
            //_context = new BotDbContext(options);
        }

        private BotDbContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<BotDbContext>()
                .UseSqlServer(_connectionString)
                .Options;
            return new BotDbContext(options);
        }

        //public async Task<UserRequest> GetOrCreateUserAsync(long telegramId)
        //{
        //    using var _context = CreateContext();

        //    var user = await _context.UserRequests.FirstOrDefaultAsync(u => u.user_id == telegramId);
        //    if (user == null)
        //    {
        //        user = new UserRequest { user_id = telegramId, count_of_requests = 0 };
        //        _context.UserRequests.Add(user);
        //        await _context.SaveChangesAsync();
        //    }
        //    return user;
        //}

        public async Task<bool> CanMakeRequestAsync(long telegramId)
        {
            using var context = CreateContext();

            var user = await context.UserRequests
                .FirstOrDefaultAsync(u => u.user_id == telegramId);

            if (user == null)
                return true;

            return user.count_of_requests < 3;
        }

        public async Task<bool> IncrementRequestAsync(long telegramId)
        {
            using var context = CreateContext();

            var user = await context.UserRequests
                .FirstOrDefaultAsync(u => u.user_id == telegramId);

            if (user == null)
            {
                user = new UserRequest
                {
                    user_id = telegramId,
                    count_of_requests = 1
                };
                context.UserRequests.Add(user);
            }
            else if (user.count_of_requests < 3)
            {
                user.count_of_requests++;
            }
            else
            {
                return false; 
            }

            await context.SaveChangesAsync();
            return true;
        }
    }
}
