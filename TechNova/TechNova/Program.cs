
using System;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

using TechNova.Services;
using TechNova.Data;
using TechNova.Models;

internal class Program
{
    private static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;

        // ==========================================
        // 1. Add MVC Services
        // ==========================================

        builder.Services.AddControllersWithViews();


        // ==========================================
        // 2. Database Context - MySQL
        // ==========================================

        var connectionString =
            builder.Configuration.GetConnectionString("DefaultConnection");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "DefaultConnection is missing from appsettings.json."
            );
        }

        builder.Services.AddDbContext<ApplicationDbContext>(options =>
            options.UseMySql(
                connectionString,
                ServerVersion.AutoDetect(connectionString)
            )
            .ConfigureWarnings(w =>
                w.Log(
                    RelationalEventId.PendingModelChangesWarning
                )
            )
        );


        // ==========================================
        // 3. ASP.NET Core Identity
        // ==========================================

        builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
        {
            options.Password.RequireDigit = true;
            options.Password.RequireLowercase = true;
            options.Password.RequireUppercase = true;
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequiredLength = 6;

            options.User.RequireUniqueEmail = true;

            options.SignIn.RequireConfirmedEmail = false;
        })
        .AddEntityFrameworkStores<ApplicationDbContext>()
        .AddDefaultTokenProviders();


        // ==========================================
        // 4. Authorization Policies
        // ==========================================

        builder.Services.AddAuthorization(options =>
        {
            options.AddPolicy(
                "AdminOnly",
                policy => policy.RequireRole("Admin")
            );

            options.AddPolicy(
                "CustomerOnly",
                policy => policy.RequireRole("Customer")
            );
        });


        // ==========================================
        // 5. HTTP Context Accessor
        // ==========================================

        builder.Services.AddHttpContextAccessor();


        // ==========================================
        // 6. Session Configuration
        // ==========================================

        builder.Services.AddDistributedMemoryCache();

        builder.Services.AddSession(options =>
        {
            options.IdleTimeout = TimeSpan.FromMinutes(30);

            options.Cookie.HttpOnly = true;

            options.Cookie.IsEssential = true;

            options.Cookie.Name = ".TechNova.Session";
        });


        // ==========================================
        // 7. Shopping Cart Service
        // ==========================================

        builder.Services.AddScoped<
            IShoppingCartService,
            ShoppingCartService
        >();

        builder.Services.AddScoped<
            IOrderService,
            OrderService
        >();

        builder.Services.AddScoped<
            IOrderInvoicePdfService,
            OrderInvoicePdfService
        >();

        builder.Services.AddScoped<IReportService, ReportService>();


        // ==========================================
        // 8. Product Service
        // ==========================================

        builder.Services.AddScoped<
            IProductService,
            ProductService
        >();


        // ==========================================
        // 9. File Service
        // ==========================================

        builder.Services.AddScoped<FileService>();

        // Email delivery for account recovery
        builder.Services.AddTransient<IEmailSender, SmtpEmailSender>();


        // ==========================================
        // 10. AutoMapper
        // ==========================================

        builder.Services.AddAutoMapper(typeof(Program));


        // ==========================================
        // 11. Build Application
        // ==========================================

        var app = builder.Build();


        // ==========================================
        // 12. HTTP Request Pipeline
        // ==========================================

        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Home/Error");

            app.UseHsts();
        }

        app.UseHttpsRedirection();

        app.UseStaticFiles();

        app.UseRouting();

        // Session middleware
        app.UseSession();

        // Authentication
        app.UseAuthentication();

        // Authorization
        app.UseAuthorization();


        // ==========================================
        // 13. Seed Database
        // ==========================================

        using (var scope = app.Services.CreateScope())
        {
            var services = scope.ServiceProvider;

            await DbInitializer.SeedAsync(services);
        }


        // ==========================================
        // 14. Default Routing
        // ==========================================

        app.MapControllerRoute(
            name: "areas",
            pattern: "{area:exists}/{controller=Dashboard}/{action=Index}/{id?}"
        );

        app.MapControllerRoute(
            name: "default",
            pattern: "{controller=Home}/{action=Index}/{id?}"
        );


        // ==========================================
        // 15. Run Application
        // ==========================================

        await app.RunAsync();
    }
}
