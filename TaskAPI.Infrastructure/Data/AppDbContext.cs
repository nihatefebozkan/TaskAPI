using Microsoft.EntityFrameworkCore; // Entity Framework Core kütüphanesini içe aktarır

namespace TaskAPI.Infrastructure.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<TaskAPI.Domain.Entity.Task> Tasks { get; set; } //in database, this will create a table for TaskAPI.Domain.Models.Entity.Task entities. The name of the table will be "Tasks" by default, but you can change it using Fluent API or Data Annotations if needed.
        public DbSet<TaskAPI.Domain.Entity.User> Users { get; set; } //in database, this will create a table for TaskAPI.Domain.Models.Entity.User entities. The name of the table will be "Users" by default, but you can change it using Fluent API or Data Annotations if needed.

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<TaskAPI.Domain.Entity.Task>()
                .HasMany(task => task.Users)
                .WithMany()
                .UsingEntity("TaskUser");

            modelBuilder.Entity<TaskAPI.Domain.Entity.User>()
                .Property(user => user.RefreshTokenHash)
                .HasMaxLength(64);

            modelBuilder.Entity<TaskAPI.Domain.Entity.User>()
                .HasIndex(user => user.RefreshTokenHash)
                .IsUnique();
        }
    }
}
