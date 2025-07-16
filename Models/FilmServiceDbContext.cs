using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace WebFilmOnline.Models;

public partial class FilmServiceDbContext : DbContext
{
    public FilmServiceDbContext()
    {
    }

    public FilmServiceDbContext(DbContextOptions<FilmServiceDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Order> Orders { get; set; }

    public virtual DbSet<OrderItem> OrderItems { get; set; }

    public virtual DbSet<Package> Packages { get; set; }

    public virtual DbSet<PointTransaction> PointTransactions { get; set; }

    public virtual DbSet<PointWallet> PointWallets { get; set; }

    public virtual DbSet<Product> Products { get; set; }

    public virtual DbSet<ProductCategory> ProductCategories { get; set; }

    public virtual DbSet<ProductStreaming> ProductStreamings { get; set; }

    public virtual DbSet<Promotion> Promotions { get; set; }

    public virtual DbSet<PromotionTarget> PromotionTargets { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<StreamingProvider> StreamingProviders { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<UserChannel> UserChannels { get; set; }

    public virtual DbSet<UserRole> UserRoles { get; set; }

    public virtual DbSet<UserSubscription> UserSubscriptions { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(e => e.OrderId).HasName("PK__Order__C3905BCF9EE46D7F");

            entity.ToTable("Order");

            entity.Property(e => e.OrderDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.PaymentMethod).HasMaxLength(50);
            entity.Property(e => e.PaymentStatus)
                .HasMaxLength(50)
                .HasDefaultValue("Pending");
            entity.Property(e => e.PaymentTransactionId).HasMaxLength(255);
            entity.Property(e => e.TotalAmount).HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.User).WithMany(p => p.Orders)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Order__UserId__5441852A");
        });

        modelBuilder.Entity<OrderItem>(entity =>
        {
            entity.HasKey(e => e.OrderItemId).HasName("PK__OrderIte__57ED0681BA49C036");

            entity.ToTable("OrderItem");

            entity.Property(e => e.FinalPrice).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Quantity).HasDefaultValue(1);
            entity.Property(e => e.UnitPrice).HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.Order).WithMany(p => p.OrderItems)
                .HasForeignKey(d => d.OrderId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__OrderItem__Order__5812160E");

            entity.HasOne(d => d.Package).WithMany(p => p.OrderItems)
                .HasForeignKey(d => d.PackageId)
                .HasConstraintName("FK__OrderItem__Packa__59FA5E80");

            entity.HasOne(d => d.Product).WithMany(p => p.OrderItems)
                .HasForeignKey(d => d.ProductId)
                .HasConstraintName("FK__OrderItem__Produ__59063A47");
        });

        modelBuilder.Entity<Package>(entity =>
        {
            entity.HasKey(e => e.PackageId).HasName("PK__Package__322035CCF8D50C40");

            entity.ToTable("Package");

            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.Name).HasMaxLength(255);
            entity.Property(e => e.Price).HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.Packages)
                .HasForeignKey(d => d.CreatedBy)
                .HasConstraintName("FK__Package__Created__4BAC3F29");

            entity.HasMany(d => d.Products).WithMany(p => p.Packages)
                .UsingEntity<Dictionary<string, object>>(
                    "PackageProduct",
                    r => r.HasOne<Product>().WithMany()
                        .HasForeignKey("ProductId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK__PackagePr__Produ__4F7CD00D"),
                    l => l.HasOne<Package>().WithMany()
                        .HasForeignKey("PackageId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK__PackagePr__Packa__4E88ABD4"),
                    j =>
                    {
                        j.HasKey("PackageId", "ProductId").HasName("PK__PackageP__F960F9A09BBC86BD");
                        j.ToTable("PackageProduct");
                    });
        });

        modelBuilder.Entity<PointTransaction>(entity =>
        {
            entity.HasKey(e => e.TransactionId).HasName("PK__PointTra__55433A6BC4AB7FC4");

            entity.ToTable("PointTransaction");

            entity.Property(e => e.Amount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Type).HasMaxLength(50);

            entity.HasOne(d => d.RelatedOrder).WithMany(p => p.PointTransactions)
                .HasForeignKey(d => d.RelatedOrderId)
                .HasConstraintName("FK__PointTran__Relat__656C112C");

            entity.HasOne(d => d.Wallet).WithMany(p => p.PointTransactions)
                .HasForeignKey(d => d.WalletId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__PointTran__Walle__6477ECF3");
        });

        modelBuilder.Entity<PointWallet>(entity =>
        {
            entity.HasKey(e => e.WalletId).HasName("PK__PointWal__84D4F90E249092A5");

            entity.ToTable("PointWallet");

            entity.HasIndex(e => e.UserId, "UQ__PointWal__1788CC4D3744FE7A").IsUnique();

            entity.Property(e => e.Balance)
                .HasDefaultValue(0m)
                .HasColumnType("decimal(18, 2)");
            entity.Property(e => e.LastUpdated)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.User).WithOne(p => p.PointWallet)
                .HasForeignKey<PointWallet>(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__PointWall__UserI__60A75C0F");
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.ProductId).HasName("PK__Product__B40CC6CD4B370F1A");

            entity.ToTable("Product");

            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.Price).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.ThumbnailUrl).HasMaxLength(500);
            entity.Property(e => e.Title).HasMaxLength(255);

            entity.HasOne(d => d.Category).WithMany(p => p.Products)
                .HasForeignKey(d => d.CategoryId)
                .HasConstraintName("FK__Product__Categor__34C8D9D1");

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.Products)
                .HasForeignKey(d => d.CreatedBy)
                .HasConstraintName("FK__Product__Created__35BCFE0A");
        });

        modelBuilder.Entity<ProductCategory>(entity =>
        {
            entity.HasKey(e => e.CategoryId).HasName("PK__ProductC__19093A0BD16B3B07");

            entity.ToTable("ProductCategory");

            entity.HasIndex(e => e.Name, "UQ__ProductC__737584F66E365CA7").IsUnique();

            entity.Property(e => e.Description).HasMaxLength(255);
            entity.Property(e => e.Name).HasMaxLength(100);
        });

        modelBuilder.Entity<ProductStreaming>(entity =>
        {
            entity.HasKey(e => e.ProductStreamingId).HasName("PK__ProductS__27303BE86E2B228D");

            entity.ToTable("ProductStreaming");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.Priority).HasDefaultValue(1);

            entity.HasOne(d => d.Channel).WithMany(p => p.ProductStreamings)
                .HasForeignKey(d => d.ChannelId)
                .HasConstraintName("FK__ProductSt__Chann__46E78A0C");

            entity.HasOne(d => d.Product).WithMany(p => p.ProductStreamings)
                .HasForeignKey(d => d.ProductId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ProductSt__Produ__44FF419A");

            entity.HasOne(d => d.Provider).WithMany(p => p.ProductStreamings)
                .HasForeignKey(d => d.ProviderId)
                .HasConstraintName("FK__ProductSt__Provi__45F365D3");
        });

        modelBuilder.Entity<Promotion>(entity =>
        {
            entity.HasKey(e => e.PromotionId).HasName("PK__Promotio__52C42FCF2AE3318A");

            entity.ToTable("Promotion");

            entity.Property(e => e.DiscountPercent).HasColumnType("decimal(5, 2)");
            entity.Property(e => e.EndDate).HasColumnType("datetime");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.MinPurchaseAmount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Name).HasMaxLength(255);
            entity.Property(e => e.StartDate).HasColumnType("datetime");

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.Promotions)
                .HasForeignKey(d => d.CreatedBy)
                .HasConstraintName("FK__Promotion__Creat__693CA210");
        });

        modelBuilder.Entity<PromotionTarget>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Promotio__3214EC074C251C5B");

            entity.ToTable("PromotionTarget");

            entity.HasIndex(e => new { e.PromotionId, e.ProductCategoryId, e.PackageId }, "UQ_PromotionTarget").IsUnique();

            entity.HasOne(d => d.Package).WithMany(p => p.PromotionTargets)
                .HasForeignKey(d => d.PackageId)
                .HasConstraintName("FK__Promotion__Packa__6EF57B66");

            entity.HasOne(d => d.ProductCategory).WithMany(p => p.PromotionTargets)
                .HasForeignKey(d => d.ProductCategoryId)
                .HasConstraintName("FK__Promotion__Produ__6E01572D");

            entity.HasOne(d => d.Promotion).WithMany(p => p.PromotionTargets)
                .HasForeignKey(d => d.PromotionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Promotion__Promo__6D0D32F4");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.RoleId).HasName("PK__Role__8AFACE1A48DC52DF");

            entity.ToTable("Role");

            entity.HasIndex(e => e.Name, "UQ__Role__737584F6B2DF97D5").IsUnique();

            entity.Property(e => e.Description).HasMaxLength(255);
            entity.Property(e => e.Name).HasMaxLength(100);
        });

        modelBuilder.Entity<StreamingProvider>(entity =>
        {
            entity.HasKey(e => e.ProviderId).HasName("PK__Streamin__B54C687DC11BD7A9");

            entity.ToTable("StreamingProvider");

            entity.Property(e => e.ApiKey).HasMaxLength(255);
            entity.Property(e => e.BaseUrl).HasMaxLength(500);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.Name).HasMaxLength(255);

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.StreamingProviders)
                .HasForeignKey(d => d.CreatedBy)
                .HasConstraintName("FK__Streaming__Creat__398D8EEE");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__User__1788CC4C7F18E169");

            entity.ToTable("User");

            entity.HasIndex(e => e.Email, "UQ__User__A9D105346A1BEB4A").IsUnique();

            entity.Property(e => e.Address).HasMaxLength(500);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Email).HasMaxLength(255);
            entity.Property(e => e.FullName).HasMaxLength(255);
            entity.Property(e => e.PasswordHash).HasMaxLength(255);
            entity.Property(e => e.Phone).HasMaxLength(20);
            entity.Property(e => e.Ssoid)
                .HasMaxLength(255)
                .HasColumnName("SSOId");
            entity.Property(e => e.Ssoprovider)
                .HasMaxLength(50)
                .HasColumnName("SSOProvider");
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .HasDefaultValue("Active");
        });

        modelBuilder.Entity<UserChannel>(entity =>
        {
            entity.HasKey(e => e.ChannelId).HasName("PK__UserChan__38C3E814E276A7AB");

            entity.ToTable("UserChannel");

            entity.Property(e => e.ApiKey).HasMaxLength(255);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.Name).HasMaxLength(255);

            entity.HasOne(d => d.Provider).WithMany(p => p.UserChannels)
                .HasForeignKey(d => d.ProviderId)
                .HasConstraintName("FK__UserChann__Provi__3F466844");

            entity.HasOne(d => d.User).WithMany(p => p.UserChannels)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__UserChann__UserI__3E52440B");
        });

        modelBuilder.Entity<UserRole>(entity =>
        {
            entity.HasKey(e => new { e.UserId, e.RoleId }).HasName("PK__UserRole__AF2760AD17EE7B6A");

            entity.ToTable("UserRole");

            entity.Property(e => e.AssignedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.Role).WithMany(p => p.UserRoles)
                .HasForeignKey(d => d.RoleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__UserRole__RoleId__2E1BDC42");

            entity.HasOne(d => d.User).WithMany(p => p.UserRoles)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__UserRole__UserId__2D27B809");
        });

        modelBuilder.Entity<UserSubscription>(entity =>
        {
            entity.HasKey(e => e.SubscriptionId).HasName("PK__UserSubs__9A2B249D6280B077");

            entity.ToTable("UserSubscription");

            entity.Property(e => e.EndDate).HasColumnType("datetime");
            entity.Property(e => e.RemainingCreditAmount)
                .HasDefaultValue(0m)
                .HasColumnType("decimal(18, 2)");
            entity.Property(e => e.StartDate).HasColumnType("datetime");

            entity.HasOne(d => d.Package).WithMany(p => p.UserSubscriptions)
                .HasForeignKey(d => d.PackageId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__UserSubsc__Packa__74AE54BC");

            entity.HasOne(d => d.User).WithMany(p => p.UserSubscriptions)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__UserSubsc__UserI__73BA3083");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
