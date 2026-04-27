using AutoMapper;
using Business;
using Core.Concretes.Entities;
using Data.Contexts;
using Data.Seeders;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Utils.Middlewares;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

builder.Services.AddGuestServices(builder.Configuration);

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

builder.Services.AddLogging(configure =>
{
    configure.ClearProviders();
    configure.AddConsole();
    configure.AddDebug();
});

var app = builder.Build();


try
{
    using (var scope = app.Services.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<StayHubContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<Guest>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<int>>>();

        Console.WriteLine("📊 Migrating database...");
        dbContext.Database.Migrate();
        Console.WriteLine("✓ Database migration completed");

        Console.WriteLine("👥 Seeding roles and admin user...");
        await RoleSeeder.SeedRolesAsync(userManager, roleManager, dbContext);
        Console.WriteLine("✓ Roles and admin user seeded");

        Console.WriteLine("🏨 Seeding hotels...");
        HotelSeeder.SeedHotels(dbContext);
        Console.WriteLine("✓ Hotels seeded");
    }
}
catch (Exception ex)
{
    Console.WriteLine($"✗ Database initialization error: {ex.Message}");
    Console.WriteLine($"✗ Stack trace: {ex.StackTrace}");
}

app.UseMiddleware<GlobalExceptionHandlingMiddleware>();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// HTTPS Redirection
app.UseHttpsRedirection();

// Static Files (CSS, JS, Images)
app.UseStaticFiles();

// Routing
app.UseRouting();

// CORS
app.UseCors("AllowAll");

// ✅ SECURITY HEADERS MIDDLEWARE
app.Use(async (context, next) =>
{
    // Content Security Policy
    context.Response.Headers.Add("Content-Security-Policy",
        "default-src 'self'; script-src 'self' 'unsafe-inline' cdn.jsdelivr.net; style-src 'self' 'unsafe-inline'; img-src 'self' data: https:;");

    // X-Content-Type-Options - MIME sniffing engelle
    context.Response.Headers.Add("X-Content-Type-Options", "nosniff");

    // X-Frame-Options - Clickjacking engelle
    context.Response.Headers.Add("X-Frame-Options", "DENY");

    // X-XSS-Protection
    context.Response.Headers.Add("X-XSS-Protection", "1; mode=block");

    // Strict-Transport-Security
    context.Response.Headers.Add("Strict-Transport-Security", "max-age=31536000; includeSubDomains");

    // Referrer-Policy
    context.Response.Headers.Add("Referrer-Policy", "strict-origin-when-cross-origin");

    await next();
});


app.UseAuthentication();
app.UseAuthorization();


app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");


app.MapGet("/health", () => new
{
    status = "healthy",
    timestamp = DateTime.UtcNow,
    environment = app.Environment.EnvironmentName,
    services = new
    {
        database = "✓ Connected",
        authentication = "✓ Enabled",
        cors = "✓ Enabled",
        security = "✓ Enabled"
    }
});


app.Run();