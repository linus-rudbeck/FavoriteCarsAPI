using Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.Sqlite;

namespace Repositories
{
    public class DatabaseConnection : DbContext
    {
        public DatabaseConnection() : base() { }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var connectionStringBuilder = new SqliteConnectionStringBuilder()
            {
                // Mode = SqliteOpenMode.ReadWriteCreate,
                DataSource = "favoritecars2.sqlite"
            };

            var connectionString = connectionStringBuilder.ToString();

            optionsBuilder.UseSqlite(connectionString);
            
            base.OnConfiguring(optionsBuilder);
        }

        public DbSet<Car> Cars { get; set; }
        public DbSet<User> Users { get; set; }
    }
}
