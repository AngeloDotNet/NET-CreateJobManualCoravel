using Example2.Entities;
using Microsoft.EntityFrameworkCore;

namespace Example2.Data;

public class AppDbContext(DbContextOptions options) : DbContext(options)
{
    public virtual DbSet<Message> Messages { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Message>(entity =>
        {
            entity.HasKey(e => e.Id);
        });
    }
}