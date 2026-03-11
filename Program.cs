using Car_Project.Data;
using Car_Project.Hubs;
using Car_Project.Models;
using Car_Project.Services;
using Car_Project.Services.Abstractions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Car_Project
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // MVC xidmətlərini qeydiyyatdan keçir
            builder.Services.AddControllersWithViews();

            // Verilənlər bazası kontekstini qeydiyyatdan keçir
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(
                    builder.Configuration.GetConnectionString("DefaultConnection")));

            // Identity xidmətlərini qeydiyyatdan keçir
            builder.Services.AddIdentity<AppUser, IdentityRole>(options =>
            {
                // Şifrə qaydaları
                options.Password.RequireDigit           = true;
                options.Password.RequiredLength         = 8;
                options.Password.RequireUppercase       = true;
                options.Password.RequireLowercase       = true;
                options.Password.RequireNonAlphanumeric = false;

                // Email unikallığı
                options.User.RequireUniqueEmail = true;

                // Lockout
                options.Lockout.DefaultLockoutTimeSpan  = TimeSpan.FromMinutes(5);
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.AllowedForNewUsers      = true;
            })
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

            // Google OAuth — AddIdentity-nin zəncirinə birbaşa əlavə et
            builder.Services.AddAuthentication()
                .AddGoogle(options =>
                {
                    options.ClientId     = builder.Configuration["Authentication:Google:ClientId"]!;
                    options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"]!;
                    options.CallbackPath = "/signin-google";

                    // Correlation cookie üçün vacibdir
                    options.CorrelationCookie.SameSite     = Microsoft.AspNetCore.Http.SameSiteMode.Lax;
                    options.CorrelationCookie.SecurePolicy  = Microsoft.AspNetCore.Http.CookieSecurePolicy.SameAsRequest;
                    options.CorrelationCookie.HttpOnly      = true;
                    options.CorrelationCookie.IsEssential   = true;
                });

            // Cookie parametrlərini konfiqurasiya et
            builder.Services.ConfigureApplicationCookie(options =>
            {
                options.LoginPath        = "/";
                options.LogoutPath       = "/Account/Logout";
                options.AccessDeniedPath = "/";
                options.SlidingExpiration = true;
                options.ExpireTimeSpan    = TimeSpan.FromDays(7);
                options.Cookie.SameSite   = Microsoft.AspNetCore.Http.SameSiteMode.Lax;
                options.Cookie.SecurePolicy = Microsoft.AspNetCore.Http.CookieSecurePolicy.SameAsRequest;
            });

            // Session (CompareItem üçün) qeydiyyatdan keçir
            builder.Services.AddDistributedMemoryCache();
            builder.Services.AddSession(options =>
            {
                options.IdleTimeout        = TimeSpan.FromMinutes(30);
                options.Cookie.HttpOnly    = true;
                options.Cookie.IsEssential = true;
            });
            builder.Services.AddHttpContextAccessor();

            // Servisləri qeydiyyatdan keçir
            builder.Services.AddScoped<IFileService,                 FileService>();
            builder.Services.AddScoped<ICarService,                  CarService>();
            builder.Services.AddScoped<IBrandService,                BrandService>();
            builder.Services.AddScoped<ICarImageService,             CarImageService>();
            builder.Services.AddScoped<ICarFeatureService,           CarFeatureService>();
            builder.Services.AddScoped<IReviewService,               ReviewService>();
            builder.Services.AddScoped<IFAQService,                  FAQService>();
            builder.Services.AddScoped<IContactMessageService,       ContactMessageService>();
            builder.Services.AddScoped<ILoanCalculationService,      LoanCalculationService>();
            builder.Services.AddScoped<ISellCarRequestService,       SellCarRequestService>();
            builder.Services.AddScoped<IServiceCenterService,        ServiceCenterService>();
            builder.Services.AddScoped<INewsletterSubscriberService, NewsletterSubscriberService>();
            builder.Services.AddScoped<ICompareItemService,          CompareItemService>();
            builder.Services.AddScoped<IWishlistService,             WishlistService>();
            // Blog
            builder.Services.AddScoped<IBlogService,     BlogService>();
            // Mağaza
            builder.Services.AddScoped<IProductService,  ProductService>();
            builder.Services.AddScoped<ICartService,     CartService>();
            // Ödəniş / Checkout
            builder.Services.AddScoped<IOrderService,    OrderService>();
            builder.Services.AddScoped<IPaymentService,  PaymentService>();
            builder.Services.AddScoped<ICouponService,   CouponService>();
            // Satış Agenti
            builder.Services.AddScoped<ISalesAgentService, SalesAgentService>();
            // Bildirişlər
            builder.Services.AddScoped<INotificationService, NotificationService>();
            // Email
            builder.Services.AddScoped<IEmailService, EmailService>();

            // AI Chatbot
            builder.Services.AddHttpClient();
            builder.Services.AddScoped<IAiChatService, AiChatService>();

            builder.Services.AddSignalR()
                .AddJsonProtocol(options =>
                {
                    options.PayloadSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
                });

            // Background Service: Zibil qutusu avtomatik təmizləmə (10 gün)
            builder.Services.AddHostedService<TrashCleanupService>();

            // Tətbiqi qur
            var app = builder.Build();

            // Rolları və SuperAdmin istifadəçisini yarat
            using (var scope = app.Services.CreateScope())
            {
                var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();

                // Rolları yarat
                foreach (var role in new[] { "SuperAdmin", "Admin", "Agent", "User" })
                {
                    if (!await roleManager.RoleExistsAsync(role))
                        await roleManager.CreateAsync(new IdentityRole(role));
                }

                // SuperAdmin istifadəçi yarat (əgər yoxdursa)
                var superAdminEmail = "superadmin@aurexo.com";
                var superAdmin = await userManager.FindByEmailAsync(superAdminEmail);
                if (superAdmin == null)
                {
                    superAdmin = new AppUser
                    {
                        FullName    = "Super Admin",
                        Email       = superAdminEmail,
                        UserName    = superAdminEmail,
                        CreatedDate = DateTime.UtcNow,
                        EmailConfirmed = true
                    };
                    var result = await userManager.CreateAsync(superAdmin, "SuperAdmin123");
                    if (result.Succeeded)
                    {
                        await userManager.AddToRoleAsync(superAdmin, "SuperAdmin");
                    }
                }
                else if (!await userManager.IsInRoleAsync(superAdmin, "SuperAdmin"))
                {
                    await userManager.AddToRoleAsync(superAdmin, "SuperAdmin");
                }
            }

            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            // Yalnız 404 status kodu üçün custom səhifə
            app.UseStatusCodePagesWithReExecute("/404Error/{0}");

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseSession();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllerRoute(
                name: "areas",
                pattern: "{area:exists}/{controller=Dashboard}/{action=Index}/{id?}");

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");
            app.MapHub<ChatHub>("/chatHub");
            app.MapHub<Car_Project.Hubs.AiChatHub>("/aiChatHub");
            app.Run();
        }
    }
}
