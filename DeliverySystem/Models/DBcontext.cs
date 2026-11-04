using DeliverySystem.Pages;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.InMemory;
using Microsoft.EntityFrameworkCore;

namespace DeliverySystem.Models
{
    public class DBcontext : DbContext
    {
        public DbSet<Account> Accounts { get; set; }

        public DbSet<Customer> Customers { get; set; }
        public DbSet<OfficeStaff> OfficeStaff { get; set; }
        public DbSet<DeliveryStaff> DeliveryStaff { get; set; }

        protected override void OnConfiguring(
            DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseInMemoryDatabase("MyDb");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Account>()
                .HasKey(a => a.Id);

            modelBuilder.Entity<Customer>()
                .HasBaseType<Account>();

            modelBuilder.Entity<OfficeStaff>()
                .HasBaseType<Account>();

            modelBuilder.Entity<DeliveryStaff>()
                .HasBaseType<Account>();
        }

        public void SeedData()
        {
            if (!Accounts.Any())
            {
                Accounts.AddRange(
                    new Customer
                    {
                        Name = "John Smith",
                        Email = "john@example.com",
                        DeliveryAddress = "Melbourne VIC"
                    },

                    new Customer
                    {
                        Name = "Jane Doe",
                        Email = "jane@example.com",
                        DeliveryAddress = "Richmond VIC"
                    },

                    new OfficeStaff
                    {
                        Name = "Michael Brown",
                        Email = "michael@example.com",
                        Department = "Operations"
                    },

                    new OfficeStaff
                    {
                        Name = "Sarah Wilson",
                        Email = "sarah@example.com",
                        Department = "Customer Service"
                    },

                    new DeliveryStaff
                    {
                        Name = "David Lee",
                        Email = "david@example.com",
                        IsAvailable = true,
                    },

                    new DeliveryStaff
                    {
                        Name = "Emily Chen",
                        Email = "emily@example.com",
                        IsAvailable = false,
                    }
                );

                SaveChanges();
            }
        }
    }
}
