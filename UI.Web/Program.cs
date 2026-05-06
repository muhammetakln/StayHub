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

// 1. MVC Servisleri
builder.Services.AddControllersWithViews();

// 2. İş Mantığı ve Veritabanı Servisleri
builder.Services.AddGuestServices(builder.Configuration);

// 3. E-Posta Yapılandırması (Kritik Bölüm)
// appsettings.json içindeki "EmailSettings" bölümünü nesneye eşler
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
// IEmailSender istendiğinde bizim yazdığımız EmailSender sınıfını verir
builder.Services.AddTransient<IEmailSender, EmailSender>();

var app = builder.Build();

// 4. Veritabanı ve Seed İşlemleri
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<StayHubContext>();
        var userManager = services.GetRequiredService<UserManager<Guest>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole<int>>>();

        context.Database.Migrate();
        await RoleSeeder.SeedRolesAsync(userManager, roleManager, context);
        HotelSeeder.SeedHotels(context);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Veritabanına başlangıç verileri (Seed) eklenirken bir hata oluştu.");
    }
}

// 5. Middleware (Ara Katman) Ayarları
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

// 6. Güvenlik Başlıkları (Development dahil her zaman aktif olması önerilir)
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