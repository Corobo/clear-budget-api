using Microsoft.EntityFrameworkCore;
using TransactionsService.Models.DB;

namespace TransactionsService.Repositories.Data
{
    public class TransactionsDbContext : DbContext
    {
        private readonly IConfiguration _configuration;

        public DbSet<Transaction> Transactions => Set<Transaction>();

        public TransactionsDbContext(DbContextOptions<TransactionsDbContext> options, IConfiguration configuration)
            : base(options)
        {
            _configuration = configuration;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);

            optionsBuilder.UseNpgsql(
                _configuration.GetConnectionString("DefaultConnection"),
                x => x.MigrationsHistoryTable("__EFMigrationsHistory", "transactions")
            );
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema("transactions");

            modelBuilder.Entity<Transaction>(entity =>
            {
                entity.ToTable("transactions");

                entity.HasKey(t => t.Id);

                entity.Property(t => t.Id)
                    .HasColumnName("id")
                    .IsRequired();

                entity.Property(t => t.UserId)
                    .HasColumnName("user_id")
                    .IsRequired();

                entity.Property(t => t.CategoryId)
                    .HasColumnName("category_id")
                    .IsRequired();

                entity.Property(t => t.Amount)
                    .HasColumnName("amount")
                    .IsRequired()
                    .HasPrecision(18, 2);

                entity.Property(t => t.Description)
                    .HasColumnName("description")
                    .HasMaxLength(500);

                entity.Property(t => t.Type)
                    .HasColumnName("type")
                    .IsRequired();

                entity.Property(t => t.Date)
                    .HasColumnName("date")
                    .IsRequired();

                entity.Property(t => t.CreatedAt)
                    .HasColumnName("created_at")
                    .IsRequired()
                    .HasDefaultValueSql("NOW()");

                entity.Property(t => t.UpdatedAt)
                    .HasColumnName("updated_at")
                    .IsRequired(false);
            });
        }
    }
}
