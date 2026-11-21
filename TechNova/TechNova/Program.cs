
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

        QuestPDF.Settings.License =
            QuestPDF.Infrastructure.LicenseType.Community;

        // 1. MVC Services
        builder.Services.AddControllersWithViews();

        // 2. MySQL Database Configuration
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
                w.Log(RelationalEventId.PendingModelChangesWarning)
            )
        );

        // 3. Identity
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

        // 4. Authorization
        builder.Services.AddAuthorization(options =>
        {
            options.AddPolicy("AdminOnly",
                policy => policy.RequireRole("Admin"));

            options.AddPolicy("CustomerOnly",
                policy => policy.RequireRole("Customer"));
        });

        // 5. HTTP Context
        builder.Services.AddHttpContextAccessor();

        // 6. Session
        builder.Services.AddDistributedMemoryCache();

        builder.Services.AddSession(options =>
        {
            options.IdleTimeout = TimeSpan.FromMinutes(30);
            options.Cookie.HttpOnly = true;
            options.Cookie.IsEssential = true;
            options.Cookie.Name = ".TechNova.Session";
        });

        // 7. Shopping Cart and Order Services
        builder.Services.AddScoped<IShoppingCartService, ShoppingCartService>();
        builder.Services.AddScoped<IOrderService, OrderService>();
        builder.Services.AddScoped<IOrderInvoicePdfService, OrderInvoicePdfService>();
        builder.Services.AddScoped<IReportService, ReportService>();

        // 8. Product Service
        builder.Services.AddScoped<IProductService, ProductService>();

        // 9. File Service
        builder.Services.AddScoped<FileService>();

        // 10. Email Service
        builder.Services.AddTransient<IEmailSender, SmtpEmailSender>();

        // 11. AutoMapper
        builder.Services.AddAutoMapper(typeof(Program));

        // 12. Build Application
        var app = builder.Build();

        // 13. Middleware
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Home/Error");
            app.UseHsts();
        }

        app.UseHttpsRedirection();

        app.UseStaticFiles();

        app.UseRouting();

        app.UseSession();

        app.UseAuthentication();

        app.UseAuthorization();

        // 14. Seed Database
        using (var scope = app.Services.CreateScope())
        {
            var services = scope.ServiceProvider;

            await DbInitializer.SeedAsync(services);
        }

        // 15. Routing
        app.MapControllerRoute(
            name: "areas",
            pattern: "{area:exists}/{controller=Dashboard}/{action=Index}/{id?}"
        );

        app.MapControllerRoute(
            name: "default",
            pattern: "{controller=Home}/{action=Index}/{id?}"
        );

        // 16. Run Application
        await app.RunAsync();
    }
}
