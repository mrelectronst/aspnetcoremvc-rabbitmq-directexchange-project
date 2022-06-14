using Microsoft.EntityFrameworkCore;

namespace AspNetCore.RabbitMQ.ExampleProject.Models.DataContext
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {

        }

        public DbSet<Product> Product { get; set; }
    }
}
