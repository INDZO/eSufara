using Microsoft.EntityFrameworkCore;
using eSufara.Model;
namespace eSufara.Data
{
    public class eSufaraDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<UserProgress> UserProgresses { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (optionsBuilder.IsConfigured)
                return;

            var connectionString = Environment.GetEnvironmentVariable("ESUFARA_CONNECTION_STRING");
            if (string.IsNullOrWhiteSpace(connectionString))
                connectionString = Environment.GetEnvironmentVariable("ESUFARA_SQL_CONNECTION") ?? @"Server=(localdb)\MSSQLLocalDB;Database=eSufaraPortfolio;Integrated Security=True;TrustServerCertificate=True";

            optionsBuilder.UseSqlServer(connectionString);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);


            modelBuilder.Entity<UserProgress>()
                .HasKey(up => new { up.UserId, up.LessonOrder });


            modelBuilder.Entity<UserProgress>()
                .HasOne(up => up.User)
                .WithMany(u => u.UserProgresses)
                .HasForeignKey(up => up.UserId)
                .OnDelete(DeleteBehavior.Cascade);  


            modelBuilder.Entity<UserProgress>()
                .Property(up => up.LessonCompleted)
                .HasDefaultValue(false);

            modelBuilder.Entity<UserProgress>()
                .Property(up => up.ExerciseCompleted)
                .HasDefaultValue(false);
        }

    }
}
