
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;

namespace DeliverySystem.Models
{
    public class DBcontext : DbContext
    {
        // Account tables
        public DbSet<Account> Accounts { get; set; } = null!;
        public DbSet<Customer> Customers { get; set; } = null!;
        public DbSet<OfficeStaff> OfficeStaff { get; set; } = null!;
        public DbSet<DeliveryStaff> DeliveryStaff { get; set; } = null!;

        // Delivery tables
        public DbSet<Delivery> Deliveries { get; set; } = null!;
        public DbSet<Parcel> Parcels { get; set; } = null!;

        // Constructor for dependency injection
        public DBcontext(DbContextOptions<DBcontext> options)
            : base(options)
        {
        }

        // Configure InMemory database
        protected override void OnConfiguring(
            DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseInMemoryDatabase("MyDb");
            }
        }

        // Configure entity relationships
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // -----------------------------
            // ACCOUNT INHERITANCE
            // -----------------------------

            modelBuilder.Entity<Account>()
                .HasKey(a => a.Id);

            // Table-per-hierarchy (TPH)
            modelBuilder.Entity<Customer>()
                .HasBaseType<Account>();

            modelBuilder.Entity<OfficeStaff>()
                .HasBaseType<Account>();

            modelBuilder.Entity<DeliveryStaff>()
                .HasBaseType<Account>();

            // Email should be unique across accounts
            modelBuilder.Entity<Account>()
                .HasIndex(a => a.Email)
                .IsUnique();

            // -----------------------------
            // DELIVERY RELATIONSHIPS
            // -----------------------------

            modelBuilder.Entity<Delivery>()
                .HasKey(d => d.Id);

            // One Customer -> Many Deliveries
            modelBuilder.Entity<Delivery>()
                .HasOne(d => d.Customer)
                .WithMany(c => c.Deliveries)
                .HasForeignKey(d => d.CustomerId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);

            // One Delivery -> Many Parcels
            modelBuilder.Entity<Parcel>()
                .HasKey(p => p.Id);

            modelBuilder.Entity<Delivery>()
                .HasMany(d => d.Parcels)
                .WithOne(p => p.Delivery)
                .HasForeignKey(p => p.DeliveryId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);

            // One DeliveryStaff -> Many Deliveries
            modelBuilder.Entity<Delivery>()
                .HasOne(d => d.DeliveryStaff)
                .WithMany()
                .HasForeignKey(d => d.DeliveryStaffId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.SetNull);
        }

        // -----------------------------
        // SEED DUMMY DATA
        // -----------------------------

        public void SeedData()
        {
            // Seed accounts only if none exist
            if (!Accounts.Any())
            {
                var customer1 = new Customer
                {
                    Name = "John Smith",
                    Email = "john@example.com",
                    Password = "Customer123!",
                    DeliveryAddress = "10 Collins Street, Melbourne"
                };

                var customer2 = new Customer
                {
                    Name = "Sarah Jones",
                    Email = "sarah@example.com",
                    Password = "Customer123!",
                    DeliveryAddress = "25 Swanston Street, Melbourne"
                };

                var officeStaff1 = new OfficeStaff
                {
                    Name = "Michael Brown",
                    Email = "michael@example.com",
                    Password = "Office123!",
                    Department = "Operations"
                };

                var officeStaff2 = new OfficeStaff
                {
                    Name = "Jessica Wilson",
                    Email = "jessica@example.com",
                    Password = "Office123!",
                    Department = "Customer Service"
                };

                var deliveryStaff1 = new DeliveryStaff
                {
                    Name = "David Lee",
                    Email = "david@example.com",
                    Password = "Delivery123!",
                    IsAvailable = true
                };

                var deliveryStaff2 = new DeliveryStaff
                {
                    Name = "Emily Chen",
                    Email = "emily@example.com",
                    Password = "Delivery123!",
                    IsAvailable = true
                };

                Accounts.AddRange(
                    customer1,
                    customer2,
                    officeStaff1,
                    officeStaff2,
                    deliveryStaff1,
                    deliveryStaff2
                );

                SaveChanges();
            }

            // Seed deliveries only if none exist
            if (!Deliveries.Any())
            {
                var customers = Customers.ToList();
                var drivers = DeliveryStaff.ToList();

                // Ensure customers exist
                if (customers.Count == 0)
                {
                    return;
                }

                var delivery1 = new Delivery
                {
                    CustomerId = customers[0].Id,
                    Status = DeliveryStatus.Pending,
                    CreatedAt = DateTime.UtcNow,

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
                    CustomerId = customers[
                        Math.Min(1, customers.Count - 1)
                    ].Id,

                    Status = DeliveryStatus.Allocated,
                    CreatedAt = DateTime.UtcNow.AddDays(-1),

                    DeliveryStaffId = drivers.Count > 0
                        ? drivers[0].Id
                        : null,

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

                var delivery3 = new Delivery
                {
                    CustomerId = customers[0].Id,
                    Status = DeliveryStatus.Delivered,
                    CreatedAt = DateTime.UtcNow.AddDays(-3),

                    DeliveryStaffId = drivers.Count > 1
                        ? drivers[1].Id
                        : null,

                    Parcels = new List<Parcel>
                    {
                        new Parcel
                        {
                            Length = 50,
                            Width = 35,
                            Height = 25,
                            Weight = 8.0
                        }
                    }
                };

                Deliveries.AddRange(
                    delivery1,
                    delivery2,
                    delivery3
                );

                SaveChanges();
            }
        }
    }
}

