using Microsoft.EntityFrameworkCore;
using IdentityService.Models;

namespace IdentityService.Data;

public class IdentityDbContext : DbContext
{
    public IdentityDbContext(DbContextOptions<IdentityDbContext> options) : base(options) { }
    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("Accounts");
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.Id)
                .HasColumnName("account_id")
                .ValueGeneratedOnAdd();
            
            entity.Property(e => e.Username)
                .HasColumnName("username")
                .IsRequired()
                .HasMaxLength(256);
            entity.HasIndex(e => e.Username).IsUnique();
            
            entity.Property(e => e.Password)
                .HasColumnName("password")
                .IsRequired();
            
            entity.Property(e => e.Email)
                .HasColumnName("email")
                .IsRequired()
                .HasMaxLength(256);
            entity.HasIndex(e => e.Email).IsUnique();
            
            entity.Property(e => e.Name)
                .HasColumnName("name")
                .HasMaxLength(256);
            
            entity.Property(e => e.PhoneNumber)
                .HasColumnName("phone_number")
                .HasMaxLength(20);
            
            entity.Property(e => e.Role)
                .HasColumnName("role")
                .IsRequired()
                .HasMaxLength(50)
                .HasDefaultValue("USER");
            
            entity.Property(e => e.IsActive)
                .HasColumnName("is_active")
                .HasDefaultValue(true);
            
            entity.Property(e => e.CreatedAt)
                .HasColumnName("created_at")
                .IsRequired();
            
            entity.Property(e => e.UpdatedAt)
                .HasColumnName("updated_at");
        });
    }
}
