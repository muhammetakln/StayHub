using Business;
using Utils.Middlewares;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllersWithViews();

// Business katmanındaki IOC.cs içinde yazdığımız tüm servisleri (Hotel, Reservation, Payment vb.) tek tek kaydeder
builder.Services.AddGuestServices(builder.Configuration);

var app = builder.Build();



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

if (!app.Environment.IsDevelopment())
{
    app.Use(async (context, next) =>
    {
        context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
        context.Response.Headers.Append("X-Frame-Options", "DENY");
        context.Response.Headers.Append("X-XSS-Protection", "1; mode=block");
        await next();
    });
}


app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();