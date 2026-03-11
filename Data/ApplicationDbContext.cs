using Car_Project.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Car_Project.Data
{
    public class ApplicationDbContext : IdentityDbContext<AppUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        // Mövcud cədvəllər
        public DbSet<Brand>                 Brands                  { get; set; }
        public DbSet<Car>                   Cars                    { get; set; }
        public DbSet<CarImage>              CarImages               { get; set; }
        public DbSet<CarFeature>            CarFeatures             { get; set; }
        public DbSet<CarFeatureMapping>     CarFeatureMappings      { get; set; }
        public DbSet<Review>                Reviews                 { get; set; }
        public DbSet<FAQ>                   FAQs                    { get; set; }
        public DbSet<ContactMessage>        ContactMessages         { get; set; }
        public DbSet<LoanCalculation>       LoanCalculations        { get; set; }
        public DbSet<SellCarRequest>        SellCarRequests         { get; set; }
        public DbSet<ServiceCenter>         ServiceCenters          { get; set; }
        public DbSet<NewsletterSubscriber>  NewsletterSubscribers   { get; set; }
        public DbSet<CompareItem>           CompareItems            { get; set; }
        public DbSet<WishlistItem>          WishlistItems           { get; set; }

        // SalesAgent
        public DbSet<SalesAgent>            SalesAgents             { get; set; }
        public DbSet<SalesAgentReview>      SalesAgentReviews       { get; set; }

        // Chat
        public DbSet<ChatMessage>           ChatMessages            { get; set; }

        // Blog
        public DbSet<BlogCategory>  BlogCategories  { get; set; }
        public DbSet<BlogPost>      BlogPosts       { get; set; }
        public DbSet<BlogTag>       BlogTags        { get; set; }
        public DbSet<BlogPostTag>   BlogPostTags    { get; set; }
        public DbSet<BlogComment>   BlogComments    { get; set; }

        // Mağaza
        public DbSet<ProductCategory>  ProductCategories  { get; set; }
        public DbSet<Product>          Products           { get; set; }
        public DbSet<ProductImage>     ProductImages      { get; set; }
        public DbSet<CartItem>         CartItems          { get; set; }

        // Sifariş / Ödəniş
        public DbSet<Order>      Orders      { get; set; }
        public DbSet<OrderItem>  OrderItems  { get; set; }
        public DbSet<Payment>    Payments    { get; set; }
        public DbSet<Coupon>     Coupons     { get; set; }

        // Bildirişlər
        public DbSet<Notification> Notifications { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // CarFeatureMapping: composite PK
            modelBuilder.Entity<CarFeatureMapping>()
                .HasKey(cfm => new { cfm.CarId, cfm.CarFeatureId });
            modelBuilder.Entity<CarFeatureMapping>()
                .HasOne(cfm => cfm.Car).WithMany(c => c.Features)
                .HasForeignKey(cfm => cfm.CarId).OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<CarFeatureMapping>()
                .HasOne(cfm => cfm.CarFeature).WithMany(cf => cf.Cars)
                .HasForeignKey(cfm => cfm.CarFeatureId).OnDelete(DeleteBehavior.Cascade);

            // Car
            modelBuilder.Entity<Car>()
                .HasOne(c => c.Brand).WithMany(b => b.Cars)
                .HasForeignKey(c => c.BrandId).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Car>()
                .Property(c => c.Price).HasColumnType("decimal(18,2)");
            modelBuilder.Entity<Car>()
                .Property(c => c.MonthlyPayment).HasColumnType("decimal(18,2)");

            // CarImage
            modelBuilder.Entity<CarImage>()
                .HasOne(ci => ci.Car).WithMany(c => c.Images)
                .HasForeignKey(ci => ci.CarId).OnDelete(DeleteBehavior.Cascade);

            // CompareItem
            modelBuilder.Entity<CompareItem>()
                .HasOne(ci => ci.Car).WithMany()
                .HasForeignKey(ci => ci.CarId).OnDelete(DeleteBehavior.Cascade);

            // WishlistItem
            modelBuilder.Entity<WishlistItem>()
                .HasOne(w => w.Car).WithMany()
                .HasForeignKey(w => w.CarId).OnDelete(DeleteBehavior.Cascade);

            // LoanCalculation
            modelBuilder.Entity<LoanCalculation>()
                .Property(l => l.CarPrice).HasColumnType("decimal(18,2)");
            modelBuilder.Entity<LoanCalculation>()
                .Property(l => l.DownPayment).HasColumnType("decimal(18,2)");
            modelBuilder.Entity<LoanCalculation>()
                .Property(l => l.InterestRate).HasColumnType("decimal(5,2)");
            modelBuilder.Entity<LoanCalculation>()
                .Property(l => l.MonthlyPayment).HasColumnType("decimal(18,2)");
            modelBuilder.Entity<LoanCalculation>()
                .Property(l => l.TotalInterestPayment).HasColumnType("decimal(18,2)");
            modelBuilder.Entity<LoanCalculation>()
                .Property(l => l.TotalLoanAmount).HasColumnType("decimal(18,2)");

            // SellCarRequest
            modelBuilder.Entity<SellCarRequest>()
                .Property(s => s.AskingPrice).HasColumnType("decimal(18,2)");

            // Unikal indekslər
            modelBuilder.Entity<NewsletterSubscriber>()
                .HasIndex(n => n.Email).IsUnique();
            modelBuilder.Entity<Brand>()
                .HasIndex(b => b.Name).IsUnique();

            // BlogPostTag: composite PK
            modelBuilder.Entity<BlogPostTag>()
                .HasKey(pt => new { pt.BlogPostId, pt.BlogTagId });
            modelBuilder.Entity<BlogPostTag>()
                .HasOne(pt => pt.BlogPost).WithMany(p => p.PostTags)
                .HasForeignKey(pt => pt.BlogPostId).OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<BlogPostTag>()
                .HasOne(pt => pt.BlogTag).WithMany(t => t.PostTags)
                .HasForeignKey(pt => pt.BlogTagId).OnDelete(DeleteBehavior.Cascade);

            // BlogPost
            modelBuilder.Entity<BlogPost>()
                .HasOne(p => p.Category).WithMany(c => c.Posts)
                .HasForeignKey(p => p.CategoryId).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<BlogPost>()
                .HasIndex(p => p.Slug).IsUnique();

            // BlogComment
            modelBuilder.Entity<BlogComment>()
                .HasOne(c => c.BlogPost).WithMany(p => p.Comments)
                .HasForeignKey(c => c.BlogPostId).OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<BlogComment>()
                .HasOne(c => c.ParentComment).WithMany(c => c.Replies)
                .HasForeignKey(c => c.ParentCommentId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);

            // Product
            modelBuilder.Entity<Product>()
                .HasOne(p => p.Category).WithMany(c => c.Products)
                .HasForeignKey(p => p.CategoryId).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Product>()
                .Property(p => p.Price).HasColumnType("decimal(18,2)");
            modelBuilder.Entity<Product>()
                .Property(p => p.OldPrice).HasColumnType("decimal(18,2)");

            // ProductImage
            modelBuilder.Entity<ProductImage>()
                .HasOne(pi => pi.Product).WithMany(p => p.Images)
                .HasForeignKey(pi => pi.ProductId).OnDelete(DeleteBehavior.Cascade);

            // CartItem
            modelBuilder.Entity<CartItem>()
                .HasOne(ci => ci.Product).WithMany(p => p.CartItems)
                .HasForeignKey(ci => ci.ProductId).OnDelete(DeleteBehavior.Cascade);

            // Order
            modelBuilder.Entity<Order>()
                .Property(o => o.SubTotal).HasColumnType("decimal(18,2)");
            modelBuilder.Entity<Order>()
                .Property(o => o.Discount).HasColumnType("decimal(18,2)");
            modelBuilder.Entity<Order>()
                .Property(o => o.ShippingCost).HasColumnType("decimal(18,2)");
            modelBuilder.Entity<Order>()
                .Property(o => o.Total).HasColumnType("decimal(18,2)");
            modelBuilder.Entity<Order>()
                .HasOne(o => o.Payment).WithOne(p => p.Order)
                .HasForeignKey<Order>(o => o.PaymentId).OnDelete(DeleteBehavior.Restrict);

            // OrderItem
            modelBuilder.Entity<OrderItem>()
                .Property(oi => oi.UnitPrice).HasColumnType("decimal(18,2)");
            modelBuilder.Entity<OrderItem>()
                .Property(oi => oi.TotalPrice).HasColumnType("decimal(18,2)");
            modelBuilder.Entity<OrderItem>()
                .HasOne(oi => oi.Order).WithMany(o => o.Items)
                .HasForeignKey(oi => oi.OrderId).OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<OrderItem>()
                .HasOne(oi => oi.Product).WithMany(p => p.OrderItems)
                .HasForeignKey(oi => oi.ProductId).OnDelete(DeleteBehavior.Restrict);

            // Payment
            modelBuilder.Entity<Payment>()
                .Property(p => p.Amount).HasColumnType("decimal(18,2)");

            // Coupon
            modelBuilder.Entity<Coupon>()
                .Property(c => c.DiscountPercent).HasColumnType("decimal(5,2)");
            modelBuilder.Entity<Coupon>()
                .Property(c => c.DiscountAmount).HasColumnType("decimal(18,2)");
            modelBuilder.Entity<Coupon>()
                .Property(c => c.MinOrderAmount).HasColumnType("decimal(18,2)");
            modelBuilder.Entity<Coupon>()
                .HasIndex(c => c.Code).IsUnique();

            // SalesAgent / SalesAgentReview
            modelBuilder.Entity<SalesAgentReview>()
                .HasOne(r => r.SalesAgent).WithMany(a => a.Reviews)
                .HasForeignKey(r => r.SalesAgentId).OnDelete(DeleteBehavior.Cascade);

            // Review → Car (optional)
            modelBuilder.Entity<Review>()
                .HasOne(r => r.Car).WithMany()
                .HasForeignKey(r => r.CarId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.SetNull);

            // Review → User (optional)
            modelBuilder.Entity<Review>()
                .HasOne(r => r.User).WithMany()
                .HasForeignKey(r => r.UserId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.SetNull);

            // Review → ParentReview (self-referencing for replies)
            modelBuilder.Entity<Review>()
                .HasOne(r => r.ParentReview).WithMany(r => r.Replies)
                .HasForeignKey(r => r.ParentReviewId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);

            // ChatMessage
            modelBuilder.Entity<ChatMessage>()
                .HasOne(m => m.Sender)
                .WithMany()
                .HasForeignKey(m => m.SenderId)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<ChatMessage>()
                .HasOne(m => m.Receiver)
                .WithMany()
                .HasForeignKey(m => m.ReceiverId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);

            // Notification
            modelBuilder.Entity<Notification>()
                .HasOne(n => n.User)
                .WithMany()
                .HasForeignKey(n => n.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
