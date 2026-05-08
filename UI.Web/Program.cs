using Business;
using Utils.Middlewares;
using Microsoft.AspNetCore.Identity;
using Core.Concretes.Entities;
using Data.Contexts;
using Data.Seeders;
using Microsoft.EntityFrameworkCore;
using Utils.Models;
using Utils.Helpers;
using Microsoft.AspNetCore.Identity.UI.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

// --- 1. GUEST SERVICES & IDENTITY AYARLARI ---
builder.Services.AddGuestServices(builder.Configuration);

// 🔥 EKLENEN GÜVENLİK AYARLARI: Var olmayan mail ve hesap güvenliği
builder.Services.Configure<IdentityOptions>(options =>
{
    // E-posta benzersiz olmalı (Aynı maille 2. kayıt yapılamaz)
    options.User.RequireUniqueEmail = true;

    // Şüpheli Giriş Denemeleri (Brute Force Korunması)
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15); // 15 dakika kilitle
    options.Lockout.MaxFailedAccessAttempts = 5; // 5 hatalı denemede bloke et
    options.Lockout.AllowedForNewUsers = true;

    // Şifre kuralları (İsteğe bağlı olarak daha da sıkılaştırabilirsin)
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 6;
});

// --- 2. COOKIE VE OTOMATİK ÇIKIŞ AYARLARI ---
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/account/login";
    options.LogoutPath = "/account/logout";
    options.AccessDeniedPath = "/account/login";

    options.ExpireTimeSpan = TimeSpan.FromMinutes(10); // 10 Dakika hareketsizlikte oturum düşer
    options.SlidingExpiration = true;

    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.Strict;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
});

builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
builder.Services.AddTransient<IEmailSender, EmailSender>();

var app = builder.Build();

// --- 3. VERİTABANI MİGRASYON VE SEEDER (Admin Ekleme Burada Yapılıyor) ---
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<StayHubContext>();
        var userManager = services.GetRequiredService<UserManager<Guest>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole<int>>>();

        context.Database.Migrate();
        // 🔥 RoleSeeder artık ilk Admin'i de oluşturacak (Bir sonraki adımda dosyasını güncelleyeceğiz)
        await RoleSeeder.SeedRolesAsync(userManager, roleManager, context);
        HotelSeeder.SeedHotels(context);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Veritabanına başlangıç verileri (Seed) eklenirken bir hata oluştu.");
    }
}

app.UseMiddleware<GlobalExceptionHandlingMiddleware>();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.Use(async (context, next) =>
{
    context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
    context.Response.Headers.Append("X-Frame-Options", "DENY");
    context.Response.Headers.Append("X-XSS-Protection", "1; mode=block");
    await next();
});

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();