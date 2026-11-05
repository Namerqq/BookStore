using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace BookStore.Models;

public partial class DbSe2038BookymartFContext : DbContext
{
    public DbSe2038BookymartFContext()
    {
    }

    public DbSe2038BookymartFContext(DbContextOptions<DbSe2038BookymartFContext> options)
        : base(options)
    {
    }

    public virtual DbSet<TbAccount> TbAccounts { get; set; }

    public virtual DbSet<TbActivityLog> TbActivityLogs { get; set; }

    public virtual DbSet<TbAddress> TbAddresses { get; set; }

    public virtual DbSet<TbCart> TbCarts { get; set; }

    public virtual DbSet<TbCartItem> TbCartItems { get; set; }

    public virtual DbSet<TbCategory> TbCategories { get; set; }

    public virtual DbSet<TbOrder> TbOrders { get; set; }

    public virtual DbSet<TbOrderDetail> TbOrderDetails { get; set; }

    public virtual DbSet<TbProduct> TbProducts { get; set; }

    public virtual DbSet<TbProductImage> TbProductImages { get; set; }

    public virtual DbSet<TbReview> TbReviews { get; set; }

    public virtual DbSet<TbSavedVoucher> TbSavedVouchers { get; set; }

    public virtual DbSet<TbVoucher> TbVouchers { get; set; }

    public virtual DbSet<TbVoucherUsage> TbVoucherUsages { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=localhost;Database=DB_SE2038_BOOKYMART_F;User Id=sa;Password=123456;TrustServerCertificate=True");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TbAccount>(entity =>
        {
            entity.HasKey(e => e.AccountId).HasName("PK__tbAccoun__46A222CD04E53507");

            entity.ToTable("tbAccount");

            entity.HasIndex(e => e.Email, "UQ__tbAccoun__AB6E61642E8BE5EB").IsUnique();

            entity.Property(e => e.AccountId).HasColumnName("account_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(sysdatetime())")
                .HasColumnName("created_at");
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .HasColumnName("email");
            entity.Property(e => e.EmailVerified).HasColumnName("email_verified");
            entity.Property(e => e.FailedLoginCount).HasColumnName("failed_login_count");
            entity.Property(e => e.FullName)
                .HasMaxLength(100)
                .HasColumnName("full_name");
            entity.Property(e => e.LockedUntil).HasColumnName("locked_until");
            entity.Property(e => e.PasswordHash)
                .HasMaxLength(255)
                .HasColumnName("password_hash");
            entity.Property(e => e.Phone)
                .HasMaxLength(20)
                .HasColumnName("phone");
            entity.Property(e => e.Role)
                .HasMaxLength(20)
                .HasColumnName("role");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValue("ACTIVE")
                .HasColumnName("status");
        });

        modelBuilder.Entity<TbActivityLog>(entity =>
        {
            entity.HasKey(e => e.LogId).HasName("PK__tbActivi__9E2397E08E29E8E3");

            entity.ToTable("tbActivityLog");

            entity.Property(e => e.LogId).HasColumnName("log_id");
            entity.Property(e => e.AccountId).HasColumnName("account_id");
            entity.Property(e => e.Action)
                .HasMaxLength(255)
                .HasColumnName("action");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(sysdatetime())")
                .HasColumnName("created_at");

            entity.HasOne(d => d.Account).WithMany(p => p.TbActivityLogs)
                .HasForeignKey(d => d.AccountId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK_ActivityLog_Account");
        });

        modelBuilder.Entity<TbAddress>(entity =>
        {
            entity.HasKey(e => e.AddressId).HasName("PK__tbAddres__CAA247C874E1B3A9");

            entity.ToTable("tbAddress");

            entity.HasIndex(e => e.AccountId, "UQ_Address_OneDefault")
                .IsUnique()
                .HasFilter("([is_default]=(1))");

            entity.Property(e => e.AddressId).HasColumnName("address_id");
            entity.Property(e => e.AccountId).HasColumnName("account_id");
            entity.Property(e => e.FullAddress)
                .HasMaxLength(255)
                .HasColumnName("full_address");
            entity.Property(e => e.IsDefault).HasColumnName("is_default");
            entity.Property(e => e.ReceiverName)
                .HasMaxLength(100)
                .HasColumnName("receiver_name");
            entity.Property(e => e.ReceiverPhone)
                .HasMaxLength(20)
                .HasColumnName("receiver_phone");

            entity.HasOne(d => d.Account).WithOne(p => p.TbAddress)
                .HasForeignKey<TbAddress>(d => d.AccountId)
                .HasConstraintName("FK_Address_Account");
        });

        modelBuilder.Entity<TbCart>(entity =>
        {
            entity.HasKey(e => e.CartId).HasName("PK__tbCart__2EF52A275394E1DC");

            entity.ToTable("tbCart");

            entity.HasIndex(e => e.AccountId, "UQ__tbCart__46A222CC3EA96508").IsUnique();

            entity.Property(e => e.CartId).HasColumnName("cart_id");
            entity.Property(e => e.AccountId).HasColumnName("account_id");

            entity.HasOne(d => d.Account).WithOne(p => p.TbCart)
                .HasForeignKey<TbCart>(d => d.AccountId)
                .HasConstraintName("FK_Cart_Account");
        });

        modelBuilder.Entity<TbCartItem>(entity =>
        {
            entity.HasKey(e => e.CartItemId).HasName("PK__tbCartIt__5D9A6C6EF307541E");

            entity.ToTable("tbCartItem");

            entity.HasIndex(e => new { e.CartId, e.ProductId }, "UQ_CartItem").IsUnique();

            entity.Property(e => e.CartItemId).HasColumnName("cart_item_id");
            entity.Property(e => e.CartId).HasColumnName("cart_id");
            entity.Property(e => e.IsSelected)
                .HasDefaultValue(true)
                .HasColumnName("is_selected");
            entity.Property(e => e.ProductId).HasColumnName("product_id");
            entity.Property(e => e.Quantity).HasColumnName("quantity");

            entity.HasOne(d => d.Cart).WithMany(p => p.TbCartItems)
                .HasForeignKey(d => d.CartId)
                .HasConstraintName("FK_CartItem_Cart");

            entity.HasOne(d => d.Product).WithMany(p => p.TbCartItems)
                .HasForeignKey(d => d.ProductId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CartItem_Product");
        });

        modelBuilder.Entity<TbCategory>(entity =>
        {
            entity.HasKey(e => e.CategoryId).HasName("PK__tbCatego__D54EE9B4D970883B");

            entity.ToTable("tbCategory");

            entity.HasIndex(e => e.Name, "UQ__tbCatego__72E12F1B62659A35").IsUnique();

            entity.Property(e => e.CategoryId).HasColumnName("category_id");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
        });

        modelBuilder.Entity<TbOrder>(entity =>
        {
            entity.HasKey(e => e.OrderId).HasName("PK__tbOrder__46596229E360AF51");

            entity.ToTable("tbOrder");

            entity.HasIndex(e => e.OrderCode, "UQ__tbOrder__99D12D3FAEF0EE90").IsUnique();

            entity.Property(e => e.OrderId).HasColumnName("order_id");
            entity.Property(e => e.AccountId).HasColumnName("account_id");
            entity.Property(e => e.CancelReason)
                .HasMaxLength(255)
                .HasColumnName("cancel_reason");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(sysdatetime())")
                .HasColumnName("created_at");
            entity.Property(e => e.DiscountAmount)
                .HasColumnType("decimal(14, 2)")
                .HasColumnName("discount_amount");
            entity.Property(e => e.OrderCode)
                .HasMaxLength(30)
                .HasColumnName("order_code");
            entity.Property(e => e.PaymentMethod)
                .HasMaxLength(30)
                .HasColumnName("payment_method");
            entity.Property(e => e.PaymentStatus)
                .HasMaxLength(20)
                .HasDefaultValue("UNPAID")
                .HasColumnName("payment_status");
            entity.Property(e => e.PhoneAttemptCount).HasColumnName("phone_attempt_count");
            entity.Property(e => e.PhoneConfirmStatus)
                .HasMaxLength(20)
                .HasDefaultValue("PENDING")
                .HasColumnName("phone_confirm_status");
            entity.Property(e => e.ReceiverName)
                .HasMaxLength(100)
                .HasColumnName("receiver_name");
            entity.Property(e => e.ReceiverPhone)
                .HasMaxLength(20)
                .HasColumnName("receiver_phone");
            entity.Property(e => e.ReturnReason)
                .HasMaxLength(500)
                .HasColumnName("return_reason");
            entity.Property(e => e.ReturnRequestedAt).HasColumnName("return_requested_at");
            entity.Property(e => e.ReturnStatus)
                .HasMaxLength(20)
                .HasColumnName("return_status");
            entity.Property(e => e.ShippingAddress)
                .HasMaxLength(255)
                .HasColumnName("shipping_address");
            entity.Property(e => e.ShippingFee)
                .HasColumnType("decimal(12, 2)")
                .HasColumnName("shipping_fee");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValue("PENDING")
                .HasColumnName("status");
            entity.Property(e => e.VoucherId).HasColumnName("voucher_id");

            entity.HasOne(d => d.Account).WithMany(p => p.TbOrders)
                .HasForeignKey(d => d.AccountId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Order_Account");

            entity.HasOne(d => d.Voucher).WithMany(p => p.TbOrders)
                .HasForeignKey(d => d.VoucherId)
                .HasConstraintName("FK_Order_Voucher");
        });

        modelBuilder.Entity<TbOrderDetail>(entity =>
        {
            entity.HasKey(e => e.OrderDetailId).HasName("PK__tbOrderD__3C5A4080432419D9");

            entity.ToTable("tbOrderDetail");

            entity.Property(e => e.OrderDetailId).HasColumnName("order_detail_id");
            entity.Property(e => e.CostPriceSnapshot)
                .HasColumnType("decimal(12, 2)")
                .HasColumnName("cost_price_snapshot");
            entity.Property(e => e.OrderId).HasColumnName("order_id");
            entity.Property(e => e.ProductId).HasColumnName("product_id");
            entity.Property(e => e.ProductNameSnapshot)
                .HasMaxLength(255)
                .HasColumnName("product_name_snapshot");
            entity.Property(e => e.Quantity).HasColumnName("quantity");
            entity.Property(e => e.UnitPrice)
                .HasColumnType("decimal(12, 2)")
                .HasColumnName("unit_price");

            entity.HasOne(d => d.Order).WithMany(p => p.TbOrderDetails)
                .HasForeignKey(d => d.OrderId)
                .HasConstraintName("FK_OrderDetail_Order");

            entity.HasOne(d => d.Product).WithMany(p => p.TbOrderDetails)
                .HasForeignKey(d => d.ProductId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_OrderDetail_Product");
        });

        modelBuilder.Entity<TbProduct>(entity =>
        {
            entity.HasKey(e => e.ProductId).HasName("PK__tbProduc__47027DF56C5B49E6");

            entity.ToTable("tbProduct");

            entity.HasIndex(e => e.Sku, "UQ__tbProduc__DDDF4BE70470E87B").IsUnique();

            entity.Property(e => e.ProductId).HasColumnName("product_id");
            entity.Property(e => e.Author)
                .HasMaxLength(150)
                .HasColumnName("author");
            entity.Property(e => e.Brand)
                .HasMaxLength(100)
                .HasColumnName("brand");
            entity.Property(e => e.CategoryId).HasColumnName("category_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(sysdatetime())")
                .HasColumnName("created_at");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.Isbn)
                .HasMaxLength(20)
                .HasColumnName("isbn");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .HasColumnName("name");
            entity.Property(e => e.Price)
                .HasColumnType("decimal(12, 2)")
                .HasColumnName("price");
            entity.Property(e => e.ProductType)
                .HasMaxLength(20)
                .HasColumnName("product_type");
            entity.Property(e => e.PublicationYear).HasColumnName("publication_year");
            entity.Property(e => e.Publisher)
                .HasMaxLength(150)
                .HasColumnName("publisher");
            entity.Property(e => e.Sku)
                .HasMaxLength(50)
                .HasColumnName("sku");
            entity.Property(e => e.Specifications).HasColumnName("specifications");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValue("ACTIVE")
                .HasColumnName("status");
            entity.Property(e => e.StockQuantity).HasColumnName("stock_quantity");
            entity.Property(e => e.UnitCost)
                .HasColumnType("decimal(12, 2)")
                .HasColumnName("unit_cost");
            entity.Property(e => e.WeightGrams).HasColumnName("weight_grams");

            entity.HasOne(d => d.Category).WithMany(p => p.TbProducts)
                .HasForeignKey(d => d.CategoryId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Product_Category");
        });

        modelBuilder.Entity<TbProductImage>(entity =>
        {
            entity.HasKey(e => e.ImageId).HasName("PK__tbProduc__DC9AC9558F09A647");

            entity.ToTable("tbProductImage");

            entity.Property(e => e.ImageId).HasColumnName("image_id");
            entity.Property(e => e.DisplayOrder).HasColumnName("display_order");
            entity.Property(e => e.ImageUrl)
                .HasMaxLength(500)
                .HasColumnName("image_url");
            entity.Property(e => e.IsThumbnail).HasColumnName("is_thumbnail");
            entity.Property(e => e.ProductId).HasColumnName("product_id");

            entity.HasOne(d => d.Product).WithMany(p => p.TbProductImages)
                .HasForeignKey(d => d.ProductId)
                .HasConstraintName("FK_ProductImage_Product");
        });

        modelBuilder.Entity<TbReview>(entity =>
        {
            entity.HasKey(e => e.ReviewId).HasName("PK__tbReview__60883D90148D314B");

            entity.ToTable("tbReview");

            entity.HasIndex(e => e.OrderDetailId, "UQ__tbReview__3C5A408184C37F5F").IsUnique();

            entity.Property(e => e.ReviewId).HasColumnName("review_id");
            entity.Property(e => e.AccountId).HasColumnName("account_id");
            entity.Property(e => e.Comment)
                .HasMaxLength(1000)
                .HasColumnName("comment");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(sysdatetime())")
                .HasColumnName("created_at");
            entity.Property(e => e.OrderDetailId).HasColumnName("order_detail_id");
            entity.Property(e => e.ProductId).HasColumnName("product_id");
            entity.Property(e => e.Rating).HasColumnName("rating");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValue("VISIBLE")
                .HasColumnName("status");

            entity.HasOne(d => d.Account).WithMany(p => p.TbReviews)
                .HasForeignKey(d => d.AccountId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Review_Account");

            entity.HasOne(d => d.OrderDetail).WithOne(p => p.TbReview)
                .HasForeignKey<TbReview>(d => d.OrderDetailId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Review_OrderDetail");

            entity.HasOne(d => d.Product).WithMany(p => p.TbReviews)
                .HasForeignKey(d => d.ProductId)
                .HasConstraintName("FK_Review_Product");
        });

        modelBuilder.Entity<TbSavedVoucher>(entity =>
        {
            entity.HasKey(e => new { e.AccountId, e.VoucherId }).HasName("PK__tbSavedV__DEA94D37D6DD89DE");

            entity.ToTable("tbSavedVoucher");

            entity.Property(e => e.AccountId).HasColumnName("account_id");
            entity.Property(e => e.VoucherId).HasColumnName("voucher_id");
            entity.Property(e => e.SavedAt)
                .HasDefaultValueSql("(sysdatetime())")
                .HasColumnName("saved_at");

            entity.HasOne(d => d.Account).WithMany(p => p.TbSavedVouchers)
                .HasForeignKey(d => d.AccountId)
                .HasConstraintName("FK_SavedVoucher_Account");

            entity.HasOne(d => d.Voucher).WithMany(p => p.TbSavedVouchers)
                .HasForeignKey(d => d.VoucherId)
                .HasConstraintName("FK_SavedVoucher_Voucher");
        });

        modelBuilder.Entity<TbVoucher>(entity =>
        {
            entity.HasKey(e => e.VoucherId).HasName("PK__tbVouche__80B6FFA8EA890ABB");

            entity.ToTable("tbVoucher");

            entity.HasIndex(e => e.Code, "UQ__tbVouche__357D4CF9548A7D48").IsUnique();

            entity.Property(e => e.VoucherId).HasColumnName("voucher_id");
            entity.Property(e => e.Code)
                .HasMaxLength(30)
                .HasColumnName("code");
            entity.Property(e => e.DiscountType)
                .HasMaxLength(20)
                .HasColumnName("discount_type");
            entity.Property(e => e.DiscountValue)
                .HasColumnType("decimal(12, 2)")
                .HasColumnName("discount_value");
            entity.Property(e => e.EndDate).HasColumnName("end_date");
            entity.Property(e => e.MaxDiscountCap)
                .HasColumnType("decimal(12, 2)")
                .HasColumnName("max_discount_cap");
            entity.Property(e => e.MaxUsageLimit).HasColumnName("max_usage_limit");
            entity.Property(e => e.MinOrderValue)
                .HasColumnType("decimal(12, 2)")
                .HasColumnName("min_order_value");
            entity.Property(e => e.PerCustomerLimit).HasColumnName("per_customer_limit");
            entity.Property(e => e.StartDate).HasColumnName("start_date");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValue("ACTIVE")
                .HasColumnName("status");
        });

        modelBuilder.Entity<TbVoucherUsage>(entity =>
        {
            entity.HasKey(e => e.UsageId).HasName("PK__tbVouche__B6B13A029FBC9880");

            entity.ToTable("tbVoucherUsage");

            entity.HasIndex(e => e.OrderId, "UQ__tbVouche__46596228BBA74EAE").IsUnique();

            entity.Property(e => e.UsageId).HasColumnName("usage_id");
            entity.Property(e => e.AccountId).HasColumnName("account_id");
            entity.Property(e => e.OrderId).HasColumnName("order_id");
            entity.Property(e => e.UsedAt)
                .HasDefaultValueSql("(sysdatetime())")
                .HasColumnName("used_at");
            entity.Property(e => e.VoucherId).HasColumnName("voucher_id");

            entity.HasOne(d => d.Account).WithMany(p => p.TbVoucherUsages)
                .HasForeignKey(d => d.AccountId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_VoucherUsage_Account");

            entity.HasOne(d => d.Order).WithOne(p => p.TbVoucherUsage)
                .HasForeignKey<TbVoucherUsage>(d => d.OrderId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_VoucherUsage_Order");

            entity.HasOne(d => d.Voucher).WithMany(p => p.TbVoucherUsages)
                .HasForeignKey(d => d.VoucherId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_VoucherUsage_Voucher");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
