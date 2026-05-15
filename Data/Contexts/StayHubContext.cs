using Core.Abstracts.Bases;
using Core.Concretes.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Data.Contexts
{
    public class StayHubContext : IdentityDbContext<Guest, IdentityRole<int>, int>
    {
        public StayHubContext(DbContextOptions<StayHubContext> options)
            : base(options)
        {
        }

        public DbSet<Guest> Guests { get; set; }
        public DbSet<Hotel> Hotels { get; set; }
        public DbSet<Room> Rooms { get; set; }
        public DbSet<Reservation> Reservations { get; set; }
        public DbSet<ReservationAddOnService> ReservationAddOnServices { get; set; }
        public DbSet<Amenity> Amenities { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<RoomImage> RoomImages { get; set; }
        public DbSet<AddOnService> AddOnServices { get; set; }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var entries = ChangeTracker.Entries<BaseEntity>();
            foreach (var entry in entries)
            {
                if (entry.State == EntityState.Deleted)
                {
                    entry.State = EntityState.Modified;
                    entry.Entity.IsDeleted = true;
                    entry.Entity.UpdatedAt = DateTime.UtcNow;
                }
            }
            return base.SaveChangesAsync(cancellationToken);
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlite("Data Source=StayHub.db;");
            }
            base.OnConfiguring(optionsBuilder);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

           
            modelBuilder.Entity<Hotel>().HasQueryFilter(h => !h.IsDeleted);
            modelBuilder.Entity<Room>().HasQueryFilter(r => !r.IsDeleted);
            modelBuilder.Entity<Review>().HasQueryFilter(rv => !rv.IsDeleted);
            modelBuilder.Entity<Reservation>().HasQueryFilter(res => !res.IsDeleted);
            modelBuilder.Entity<AddOnService>().HasQueryFilter(a => !a.IsDeleted);

            modelBuilder.Entity<Guest>(entity =>
            {
                entity.ToTable("AspNetUsers");
                entity.Property(e => e.FirstName).HasMaxLength(50).IsRequired();
                entity.Property(e => e.LastName).HasMaxLength(50).IsRequired();
                entity.Property(e => e.IdentificationNumber).HasMaxLength(11).IsRequired();
                entity.Property(e => e.DateOfBirth).IsRequired();
                entity.Property(e => e.Country).HasMaxLength(50).IsRequired();
                entity.Property(e => e.Address).HasMaxLength(200).IsRequired();
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP").IsRequired();

                entity.HasMany(g => g.Reservations)
                    .WithOne(r => r.Guest)
                    .HasForeignKey(r => r.GuestId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasMany(g => g.Reviews)
                    .WithOne(r => r.Guest)
                    .HasForeignKey(r => r.GuestId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Hotel>(entity =>
            {
                entity.HasKey(h => h.Id);
                entity.Property(h => h.Name).HasMaxLength(100).IsRequired();
                entity.Property(h => h.Description).HasMaxLength(1000);
                entity.Property(h => h.City).HasMaxLength(50).IsRequired();
                entity.Property(h => h.Country).HasMaxLength(50).IsRequired();
                entity.Property(h => h.Address).HasMaxLength(200).IsRequired();
                entity.Property(h => h.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.HasMany(h => h.Rooms)
                    .WithOne(r => r.Hotel)
                    .HasForeignKey(r => r.HotelId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(h => h.Reviews)
                    .WithOne(r => r.Hotel)
                    .HasForeignKey(r => r.HotelId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Room>(entity =>
            {
                entity.HasKey(r => r.Id);
                entity.Property(r => r.RoomNumber).HasMaxLength(10).IsRequired();
                entity.Property(r => r.Description).HasMaxLength(500);
                entity.Property(r => r.Price).HasPrecision(18, 2).IsRequired();
                entity.Property(r => r.Capacity).IsRequired();
                entity.Property(r => r.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.HasMany(r => r.Reservations)
                    .WithOne(res => res.Room)
                    .HasForeignKey(res => res.RoomId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasMany(r => r.RoomImage)
                    .WithOne(ri => ri.Room)
                    .HasForeignKey(ri => ri.RoomId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Reservation>(entity =>
            {
                entity.HasKey(r => r.Id);
                entity.Property(r => r.CheckInDate).IsRequired();
                entity.Property(r => r.CheckOutDate).IsRequired();
                entity.Property(r => r.TotalPrice).HasPrecision(18, 2).IsRequired();
                entity.Property(r => r.Status).HasMaxLength(50).IsRequired();
                entity.Property(r => r.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.HasMany(r => r.SelectedServices)
                    .WithOne(ras => ras.Reservation)
                    .HasForeignKey(ras => ras.ReservationId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(r => r.Payments)
                    .WithOne(p => p.Reservation)
                    .HasForeignKey(p => p.ReservationId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Payment>(entity =>
            {
                entity.HasKey(p => p.Id);
                entity.HasOne(p => p.Reservation)
                      .WithMany(r => r.Payments)
                      .HasForeignKey(p => p.ReservationId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.Property(p => p.PaymentReference).HasMaxLength(100).IsRequired();
                entity.Property(p => p.Amount).HasPrecision(18, 2).IsRequired();
                entity.Property(p => p.PaymentMethod).HasMaxLength(50).IsRequired();
                entity.Property(p => p.Status).HasConversion<string>().HasMaxLength(20).IsRequired();
                entity.Property(p => p.TransactionId).HasMaxLength(100);
                entity.Property(p => p.Notes).HasMaxLength(500);
                entity.Property(p => p.PaymentDate).HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.Property(p => p.IsDeleted).HasDefaultValue(false);
                entity.Property(p => p.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            });

            modelBuilder.Entity<Review>(entity =>
            {
                entity.HasKey(r => r.Id);
                entity.Property(r => r.Rating).IsRequired();
                entity.Property(r => r.Comment).HasMaxLength(500);
                entity.Property(r => r.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            });

            modelBuilder.Entity<RoomImage>(entity =>
            {
                entity.HasKey(ri => ri.Id);
                entity.Property(ri => ri.ImageUrl).HasMaxLength(500).IsRequired();
            });

            modelBuilder.Entity<AddOnService>(entity =>
            {
                entity.HasKey(a => a.Id);
                entity.Property(a => a.Name).HasMaxLength(100).IsRequired();
                entity.Property(a => a.Description).HasMaxLength(500);
                entity.Property(a => a.Price).HasPrecision(18, 2).IsRequired();
                entity.Property(a => a.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.HasMany(a => a.ReservationAddOnServices)
                    .WithOne(ras => ras.AddOnService)
                    .HasForeignKey(ras => ras.AddOnServiceId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<ReservationAddOnService>(entity =>
            {
                entity.HasKey(ras => new { ras.ReservationId, ras.AddOnServiceId });
                entity.Property(ras => ras.Quantity).IsRequired();
                entity.Property(ras => ras.Price).HasPrecision(18, 2).IsRequired();
            });

            modelBuilder.Entity<Amenity>(entity =>
            {
                entity.HasKey(a => a.Id);
                entity.Property(a => a.Name).HasMaxLength(100).IsRequired();
                entity.Property(a => a.Description).HasMaxLength(500);
            });
        }
    }
}