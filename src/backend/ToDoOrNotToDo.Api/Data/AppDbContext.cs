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
                .HasDefaultValue(false);
            
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
}