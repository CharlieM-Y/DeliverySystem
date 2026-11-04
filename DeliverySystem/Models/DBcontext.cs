using DeliverySystem.Pages;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.InMemory;
namespace DeliverySystem.Models
{
    public class DBcontext : DbContext
    {
        public DbSet<Account> Accounts { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseInMemoryDatabase("MyDb");
        }

        public void SeedData()
        {
            // Only add dummy accounts if the database is empty
            if (!Accounts.Any())
            {
                Accounts.AddRange(
                    new Account { Name = "John Smith", Age = 25 },
                    new Account { Name = "Jane Doe", Age = 30 },
                    new Account { Name = "Michael Brown", Age = 22 },
                    new Account { Name = "Sarah Wilson", Age = 28 },
                    new Account { Name = "David Lee", Age = 35 }
                );

                SaveChanges();
            }
        }

    }
}
