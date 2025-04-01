using Microsoft.EntityFrameworkCore;
using CategoriesService.Models.DB;
using Microsoft.Extensions.Configuration;

namespace CategoriesService.Repositories.Data;

public class CategoriesDbContext : DbContext
{

    private readonly IConfiguration _configuration;
    public DbSet<Category> Categories => Set<Category>();

    public CategoriesDbContext(DbContextOptions<CategoriesDbContext> options, IConfiguration configuration) :
        base(options)
    {
        _configuration = configuration;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);
        optionsBuilder.UseNpgsql(_configuration.GetConnectionString("DefaultConnection"),
            x => x.MigrationsHistoryTable("__EFMigrationsHistory", "categories"));
        optionsBuilder.UseNpgsql(_configuration.GetConnectionString("DefaultConnection"));
    }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("categories");

        modelBuilder.Entity<Category>(entity =>
        {
            // Table name
            entity.ToTable("Categories");

            // Primary Key
            entity.HasKey(c => c.Id);

            // Properties
            entity.Property(c => c.Id)
                .IsRequired();

            entity.Property(c => c.Name)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(c => c.Color)
                .IsRequired()
                .HasMaxLength(20)
                .HasDefaultValue("#000000");

            entity.Property(c => c.UserId)
                .IsRequired(false); // Nullable → admin category

            entity.Property(c => c.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("NOW()");
        });
    }
}
