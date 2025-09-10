using Microsoft.EntityFrameworkCore;

namespace ToDoOrNotToDo.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<TaskEntity> Tasks { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<TaskEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.Id)
                .ValueGeneratedOnAdd();
            
            entity.Property(e => e.Title)
                .IsRequired()
                .HasMaxLength(100)
                .IsUnicode(false);
            
            entity.Property(e => e.IsCompleted)
                .IsRequired()
                .HasDefaultValue(false)
                .ValueGeneratedNever();
            
            entity.Property(e => e.CreatedAt)
                .IsRequired()
                .HasColumnType("TEXT");
            
            entity.Property(e => e.UpdatedAt)
                .IsRequired()
                .HasColumnType("TEXT");
            
            entity.Property(e => e.CompletedAt)
                .HasColumnType("TEXT");
            
            // Add indexes
            entity.HasIndex(e => e.CreatedAt)
                .HasDatabaseName("IX_Tasks_CreatedAt");
            
            entity.HasIndex(e => e.CompletedAt)
                .HasDatabaseName("IX_Tasks_CompletedAt");
        });
    }

    public static void Seed(AppDbContext context)
    {
        // Check if Tasks table is empty
        if (context.Tasks.Any())
        {
            return; // Database already has data, no need to seed
        }

        var now = DateTime.UtcNow;
        
        var tasks = new List<TaskEntity>
        {
            new TaskEntity
            {
                Title = "Explore UI/UX",
                IsCompleted = false,
                CreatedAt = now.AddMinutes(-5),
                UpdatedAt = now.AddMinutes(-5),
                CompletedAt = null
            },
            new TaskEntity
            {
                Title = "Review front-end design",
                IsCompleted = false,
                CreatedAt = now.AddMinutes(-4),
                UpdatedAt = now.AddMinutes(-4),
                CompletedAt = null
            },
            new TaskEntity
            {
                Title = "Review back-end design",
                IsCompleted = false,
                CreatedAt = now.AddMinutes(-3),
                UpdatedAt = now.AddMinutes(-3),
                CompletedAt = null
            },
            new TaskEntity
            {
                Title = "Prepare job offer",
                IsCompleted = false,
                CreatedAt = now.AddMinutes(-2),
                UpdatedAt = now.AddMinutes(-2),
                CompletedAt = null
            },
            new TaskEntity
            {
                Title = "Start task management application",
                IsCompleted = true,
                CreatedAt = now.AddMinutes(-1),
                UpdatedAt = now.AddMinutes(-1),
                CompletedAt = now
            }
        };

        context.Tasks.AddRange(tasks);
        context.SaveChanges();
    }
}