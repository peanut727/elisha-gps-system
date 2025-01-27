using elisha_gps_reservation_system.Data;
using gps_tracking_system.Data;
using Microsoft.EntityFrameworkCore;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    // Database sets
    public DbSet<User> Users { get; set; }
    public DbSet<Vehicle> Vehicles { get; set; }
    public DbSet<ReservationModel> Reservation { get; set; }
    public DbSet<VehicleReview> VehicleReviews { get; set; }
    public DbSet<Message> Messages { get; set; }
    public DbSet<NotificationModel> Notifications { get; set; }
    public DbSet<IncidentLog> IncidentLogs { get; set; }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure the Message entity
        modelBuilder.Entity<Message>(entity =>
        {
            entity.HasKey(m => m.MessageId);

            entity.HasOne(m => m.Sender)
                .WithMany()
                .HasForeignKey(m => m.SenderId)
                .OnDelete(DeleteBehavior.Restrict); // Prevent cascade delete

            entity.HasOne(m => m.Receiver)
                .WithMany()
                .HasForeignKey(m => m.ReceiverId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Additional configurations for other entities can go here.
    }
}