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
                entity.Property(e => e.CreateDate).HasColumnName("create_date").HasDefaultValueSql("GETDATE()");
                entity.Property(e => e.SpecsId).HasColumnName("specs_id");

                // 1:1 with Specs
                entity.HasOne(p => p.Specs)
                .WithOne(s => s.Product)
                .HasForeignKey<Product>(p => p.SpecsId)   
                .HasPrincipalKey<Specs>(s => s.SpecsId)
                .HasConstraintName("FK_Product_Specs")
                .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(p => p.AddedByUser)
                      .WithMany(u => u.AddedProducts)
                      .HasForeignKey(p => p.AddedByUserId)
                      .HasConstraintName("FK_Product_User_added_by_user_id");
            });

            // Specs configuration
            modelBuilder.Entity<Specs>(entity =>
            {
                entity.ToTable("Specs");
                entity.HasKey(e => e.SpecsId);
                entity.Property(e => e.SpecsId).HasColumnName("specs_id").ValueGeneratedNever();

                entity.Property(e => e.Processor).HasColumnName("processor");
                entity.Property(e => e.RamGb).HasColumnName("ram_gb");
                entity.Property(e => e.RamType).HasColumnName("ram_type");
                entity.Property(e => e.StorageGb).HasColumnName("storage_gb");
                entity.Property(e => e.StorageType).HasColumnName("storage_type");
                entity.Property(e => e.StorageInterface).HasColumnName("storage_interface");
                entity.Property(e => e.Gpu).HasColumnName("gpu");
                entity.Property(e => e.GpuType).HasColumnName("gpu_type");
                entity.Property(e => e.BatteryCapacityWh).HasColumnName("battery_capacity_wh");
                entity.Property(e => e.BatteryLifeHours).HasColumnName("battery_life_hours");
                entity.Property(e => e.CoolingSystem).HasColumnName("cooling_system");
                entity.Property(e => e.DisplayResolution).HasColumnName("display_resolution");
                entity.Property(e => e.DisplayRefreshRate).HasColumnName("display_refresh_rate");
                entity.Property(e => e.PortsDescription).HasColumnName("ports_description");
                entity.Property(e => e.WeightKg).HasColumnName("weight_kg");
                entity.Property(e => e.Dimensions).HasColumnName("dimensions");
                entity.Property(e => e.OperatingSystem).HasColumnName("operating_system");
                entity.Property(e => e.WarrantyMonths).HasColumnName("warranty_months");
                entity.Property(e => e.AdditionalFeatures).HasColumnName("additional_features");
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

                e.HasOne(r => r.User)
                    .WithMany(u => u.Reviews)
                    .HasForeignKey(r => r.UserId)
                    .HasConstraintName("FK_Review_User_user_id")
                    .OnDelete(DeleteBehavior.Cascade);
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