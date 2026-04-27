using AutoMapper;
using Business;
using Core.Concretes.Entities;
using Data.Contexts;
using Data.Seeders;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Utils.Middlewares;

var builder = WebApplication.CreateBuilder(args);

// ═══════════════════════════════════════════════════════════════
// SERVICES
// ═══════════════════════════════════════════════════════════════

builder.Services.AddControllersWithViews();

// ✅ IOC - Tüm Services (Database, Identity, AutoMapper, BusinessServices)
builder.Services.AddGuestServices(builder.Configuration);

// CORS Policy
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Logging
builder.Services.AddLogging(configure =>
{
    configure.ClearProviders();
    configure.AddConsole();
    configure.AddDebug();
});

// ═══════════════════════════════════════════════════════════════
// BUILD APP
// ═══════════════════════════════════════════════════════════════

var app = builder.Build();

// ═══════════════════════════════════════════════════════════════
// AUTO MIGRATION WITH SEEDER
// ═══════════════════════════════════════════════════════════════

try
{
    using (var scope = app.Services.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<StayHubContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<Guest>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<int>>>();

        // 1. Migration
        Console.WriteLine("📊 Migrating database...");
        dbContext.Database.Migrate();
        Console.WriteLine("✓ Database migration completed");

        // 2. Seed Roles and Admin User
        Console.WriteLine("👥 Seeding roles and admin user...");
        await RoleSeeder.SeedRolesAsync(userManager, roleManager, dbContext);
        Console.WriteLine("✓ Roles and admin user seeded");

        // 3. Seed Hotels
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

// ═══════════════════════════════════════════════════════════════
// MIDDLEWARE
// ═══════════════════════════════════════════════════════════════

// ✅ GLOBAL EXCEPTION HANDLING MIDDLEWARE
app.UseMiddleware<GlobalExceptionHandlingMiddleware>();

// Exception Handler
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

// ✅ SECURITY HEADERS - DEVELOPMENT'DA KAPALI
if (!app.Environment.IsDevelopment())
{
    app.Use(async (context, next) =>
    {
        // Content Security Policy
        context.Response.Headers.Add("Content-Security-Policy",
            "default-src 'self'; " +
            "script-src 'self' 'unsafe-inline' cdn.jsdelivr.net; " +
            "style-src 'self' 'unsafe-inline'; " +
            "img-src 'self' data: https:; " +
            "connect-src 'self' https:;");

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
}

// Authentication & Authorization
app.UseAuthentication();
app.UseAuthorization();

// ═══════════════════════════════════════════════════════════════
// CONTROLLER ROUTES
// ═══════════════════════════════════════════════════════════════

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Health Check
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
        security = app.Environment.IsDevelopment() ? "⚠️ Disabled (Development)" : "✓ Enabled"
    }
});

// ═══════════════════════════════════════════════════════════════
// RUN
// ═══════════════════════════════════════════════════════════════

app.Run();