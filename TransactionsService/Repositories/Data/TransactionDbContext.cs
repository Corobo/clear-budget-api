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
                entity.ToTable("Transactions");

                entity.HasKey(t => t.Id);

                entity.Property(t => t.Id)
                    .IsRequired();

                entity.Property(t => t.UserId)
                    .IsRequired();

                entity.Property(t => t.CategoryId)
                    .IsRequired();

                entity.Property(t => t.Amount)
                    .IsRequired()
                    .HasPrecision(18, 2);

                entity.Property(t => t.Description)
                    .HasMaxLength(500);

                entity.Property(t => t.Type)
                    .IsRequired();

                entity.Property(t => t.Date)
                    .IsRequired();

                entity.Property(t => t.CreatedAt)
                    .IsRequired()
                    .HasDefaultValueSql("NOW()");

                entity.Property(t => t.UpdatedAt)
                    .IsRequired(false);
            });
        }
    }
}
