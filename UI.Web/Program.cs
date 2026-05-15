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
using System.Globalization;
using Microsoft.AspNetCore.Localization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

builder.Services.AddGuestServices(builder.Configuration);

builder.Services.Configure<IdentityOptions>(options =>
{
    options.User.RequireUniqueEmail = true;

    // ✅ BU AYARLAR SAHTE MAİLLERİ ENGELLER:
    // Kullanıcı kayıt olsa bile, veritabanındaki "EmailConfirmed" alanı true 
    // yapılmadan giriş (Login) işlemi başarılı sonuçlanmaz.
    options.SignIn.RequireConfirmedEmail = true;
    options.SignIn.RequireConfirmedAccount = true;
    options.SignIn.RequireConfirmedPhoneNumber = false;

    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;

    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AtLeast18", policy =>
        policy.RequireAssertion(context =>
            context.User.HasClaim(c => c.Type == "DateOfBirth") &&
            DateTime.TryParse(context.User.FindFirst("DateOfBirth").Value, out DateTime birthDate) &&
            birthDate <= DateTime.Now.AddYears(-18)));
});

builder.Services.AddScoped<IPasswordHasher<Hotel>, PasswordHasher<Hotel>>();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/account/login";
    options.LogoutPath = "/account/logout";
    options.AccessDeniedPath = "/account/login";

    options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
    options.SlidingExpiration = true;

    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
});

builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
builder.Services.AddTransient<IEmailSender, EmailSender>();

var app = builder.Build();

var turkishCulture = new CultureInfo("tr-TR");
CultureInfo.DefaultThreadCurrentCulture = turkishCulture;
CultureInfo.DefaultThreadCurrentUICulture = turkishCulture;

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

app.UseMiddleware<GlobalExceptionHandlingMiddleware>();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
else
{
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRequestLocalization(new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture("tr-TR"),
    SupportedCultures = new[] { turkishCulture },
    SupportedUICultures = new[] { turkishCulture }
});

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.Use(async (context, next) =>
{
    context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
    context.Response.Headers.Append("X-Frame-Options", "SAMEORIGIN");
    context.Response.Headers.Append("X-XSS-Protection", "1; mode=block");
    context.Response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");
    await next();
});

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();