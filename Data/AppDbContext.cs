using BusBookingSystem.API.Models;
using Microsoft.EntityFrameworkCore;

namespace BusBookingSystem.API.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        // Register all tables
        public DbSet<User> Users { get; set; }
        public DbSet<BusOperator> BusOperators { get; set; }
        public DbSet<Bus> Buses { get; set; }
        public DbSet<SeatLayout> SeatLayouts { get; set; }
        public DbSet<Models.Route> Routes { get; set; }
        public DbSet<Stop> Stops { get; set; }
        public DbSet<Schedule> Schedules { get; set; }
        public DbSet<Trip> Trips { get; set; }
        public DbSet<Booking> Bookings { get; set; }
        public DbSet<BookedSeat> BookedSeats { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<Cancellation> Cancellations { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<Offer> Offers { get; set; }
        public DbSet<Notification> Notifications { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // 1. Configure Decimal Precision
            var decimalProps = new[]
            {
        "Rating", "DistanceKm", "EstimatedDurationHours", "BaseFare",
        "TotalFare", "Amount", "RefundAmount", "CancellationCharges",
        "DiscountValue", "MinBookingAmount", "MaxDiscount"
    };

            foreach (var entity in modelBuilder.Model.GetEntityTypes())
            {
                foreach (var prop in entity.GetProperties())
                {
                    if (decimalProps.Contains(prop.Name))
                        prop.SetColumnType("decimal(18,2)");
                }
            }

            // 2. Configure Relationships & Fix Cascade Paths

            // User -> Operator (1:1)
            modelBuilder.Entity<User>()
                .HasOne(u => u.BusOperator)
                .WithOne(o => o.User)
                .HasForeignKey<BusOperator>(o => o.UserId)
                .OnDelete(DeleteBehavior.Cascade); // Deleting User deletes Operator profile

            // FIX: Prevent Multiple Cascade Paths for Bookings

            // Trip -> Bookings
            modelBuilder.Entity<Booking>()
                .HasOne(b => b.Trip)
                .WithMany(t => t.Bookings)
                .HasForeignKey(b => b.TripId)
                .OnDelete(DeleteBehavior.Restrict); // STOP Cascade here

            // User -> Bookings
            modelBuilder.Entity<Booking>()
                .HasOne(b => b.User)
                .WithMany(u => u.Bookings)
                .HasForeignKey(b => b.UserId)
                .OnDelete(DeleteBehavior.Restrict); // STOP Cascade here

            // Booking -> Payment (1:1)
            modelBuilder.Entity<Booking>()
                .HasOne(b => b.Payment)
                .WithOne(p => p.Booking)
                .HasForeignKey<Payment>(p => p.BookingId)
                .OnDelete(DeleteBehavior.Cascade);

            // Booking -> Cancellation (1:1)
            modelBuilder.Entity<Booking>()
                .HasOne(b => b.Cancellation)
                .WithOne(c => c.Booking)
                .HasForeignKey<Cancellation>(c => c.BookingId)
                .OnDelete(DeleteBehavior.Cascade);

            // 3. Configure Unique Indexes
            modelBuilder.Entity<User>().HasIndex(u => u.Email).IsUnique();
            modelBuilder.Entity<Booking>().HasIndex(b => b.BookingReference).IsUnique();
            modelBuilder.Entity<Bus>().HasIndex(b => b.BusNumber).IsUnique();
            modelBuilder.Entity<Offer>().HasIndex(o => o.OfferCode).IsUnique();
        }
    }
}