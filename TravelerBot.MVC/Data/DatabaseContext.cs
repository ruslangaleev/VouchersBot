using System.Data.Entity;
using TravelerBot.MVC.Data.Models;

namespace TravelerBot.MVC.Data
{
    public class DatabaseContext : DbContext
    {
        public DbSet<User> Users { get; set; }

        public DbSet<UserState> UserStates { get; set; }

        public DbSet<Role> Roles { get; set; }
    }
}