using Microsoft.EntityFrameworkCore;
using LopTopWebApi.Domain.Entities;

namespace LaptopsApi.Infrastructure.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<Product> Products { get; set; }
        public DbSet<Specs> Specs { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<User> Users { get; set; }

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

            modelBuilder.Entity<Review>(e =>
            {
                e.ToTable("Review");
                e.HasKey(x => x.ReviewId);
                e.Property(x => x.ReviewId).HasColumnName("review_id").ValueGeneratedNever();
                e.Property(x => x.ProductId).HasColumnName("product_id");
                e.Property(x => x.UserId).HasColumnName("user_id");
                e.Property(x => x.ParentReviewId).HasColumnName("parent_review_id");
                e.Property(x => x.Rating).HasColumnName("rating");
                e.Property(x => x.Comment).HasColumnName("comment");
                e.Property(x => x.ReviewDate).HasColumnName("review_date");
                e.Property(x => x.EditedAt).HasColumnName("edited_at");
                e.Property(x => x.IsDeleted).HasColumnName("is_deleted");
            });

            modelBuilder.Entity<User>(e =>
            {
                e.ToTable("User");
                e.HasKey(x => x.UserId);
                e.Property(x => x.UserId).HasColumnName("user_id").ValueGeneratedNever();
                e.Property(x => x.Username).HasColumnName("username").HasMaxLength(255);
                e.Property(x => x.Email).HasColumnName("email").HasMaxLength(255);
                e.Property(x => x.PasswordHash).HasColumnName("password_hash");
                e.Property(x => x.RegistrationDate).HasColumnName("registration_date");
                e.Property(x => x.FirstName).HasColumnName("first_name");
                e.Property(x => x.LastName).HasColumnName("last_name");
            });
        }
    }
}