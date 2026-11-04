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

        // New DbSets
        public DbSet<Delivery> Deliveries { get; set; }
        public DbSet<Parcel> Parcels { get; set; }

        protected override void OnConfiguring(
            DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseInMemoryDatabase("MyDb");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Account>()
                .HasKey(a => a.Id);

            modelBuilder.Entity<Delivery>()
                .HasMany(d => d.Parcels)
                .WithOne(p => p.Delivery)
                .HasForeignKey(p => p.DeliveryId);

            modelBuilder.Entity<Delivery>()
                .HasOne(d => d.DeliveryStaff)
                .WithMany()
                .HasForeignKey(d => d.DeliveryStaffId);
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

                if (!Deliveries.Any())
                {
                    var delivery1 = new Delivery
                    {
                        Status = DeliveryStatus.Pending,
                        Parcels = new List<Parcel>
                    {
                        new Parcel
                        {
                            Length = 30,
                            Width = 20,
                            Height = 15,
                            Weight = 2.5
                        },
                        new Parcel
                        {
                            Length = 40,
                            Width = 30,
                            Height = 20,
                            Weight = 5.0
                        }
                    }
                    };

                    var delivery2 = new Delivery
                    {
                        Status = DeliveryStatus.Delivered,
                        Parcels = new List<Parcel>
                        {
                            new Parcel
                            {
                                Length = 15,
                                Width = 10,
                                Height = 10,
                                Weight = 1.2
                            }
                        }
                    };

                    Deliveries.AddRange(delivery1, delivery2);

                }

                    SaveChanges();
            }
        }
    }
}
