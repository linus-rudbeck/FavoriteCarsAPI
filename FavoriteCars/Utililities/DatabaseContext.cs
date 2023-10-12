using FavoriteCars.Models;
using Microsoft.EntityFrameworkCore;

namespace FavoriteCars.Utililities
{
    public class DatabaseContext : DbContext
    {
        public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options)
        { 
        }

        public DbSet<Car> Cars { get; set; }
        public DbSet<User> Users { get; set; }
    }
}
