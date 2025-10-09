using Microsoft.EntityFrameworkCore;
using LopTopWebApi.Domain.Entities;

namespace LaptopsApi.Infrastructure.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<Product> Products { get; set; }
        public DbSet<Specs> Specs { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Product configuration
            modelBuilder.Entity<Product>(entity =>
            {
                entity.ToTable("Product");
                entity.HasKey(e => e.ProductId);
                entity.Property(e => e.ProductId).HasColumnName("product_id").ValueGeneratedNever();
                entity.Property(e => e.Name).HasColumnName("name").HasMaxLength(100);
                entity.Property(e => e.Price).HasColumnName("price").HasPrecision(10, 2);
                entity.Property(e => e.Brand).HasColumnName("brand").HasMaxLength(50);
                entity.Property(e => e.ScreenSize).HasColumnName("screen_size").HasPrecision(4, 1);
                entity.Property(e => e.Description).HasColumnName("description").HasColumnType("nvarchar(max)");
                entity.Property(e => e.AddedByUserId).HasColumnName("added_by_user_id");
                entity.Property(e => e.AddedDate).HasColumnName("added_date");
                entity.Property(e => e.SpecsId).HasColumnName("specs_id");
                entity.Property(e => e.AddedDate).HasColumnName("added_date");
                entity.Property(e => e.CreateDate).HasColumnName("create_date").HasDefaultValueSql("GETDATE()");

                // 1:1 with Specs
                entity.HasOne(p => p.Specs)
                .WithOne(s => s.Product)
                .HasForeignKey<Product>(p => p.SpecsId)   
                .HasPrincipalKey<Specs>(s => s.SpecsId)
                .HasConstraintName("FK_Product_Specs")
                .OnDelete(DeleteBehavior.SetNull);
            });

            // Specs configuration
            modelBuilder.Entity<Specs>(entity =>
            {
                entity.ToTable("Specs");
                entity.HasKey(e => e.SpecsId);
                entity.Property(e => e.SpecsId).HasColumnName("specs_id").ValueGeneratedNever();
                entity.Property(e => e.RamGb).HasColumnName("ram_gb");
            });
        }
    }
}