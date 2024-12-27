using MobSecLab.Models; // DbContext için gerekli namespace
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http.Features;

var builder = WebApplication.CreateBuilder(args);

// Kestrel ile ilgili limitleri ayarlayalım
builder.WebHost.ConfigureKestrel(options =>
{
    // 500 MB olarak ayarladık, isterseniz daha da yükseltebilirsiniz (byte cinsinden)
    options.Limits.MaxRequestBodySize = 500 * 1024 * 1024; 
});

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.Configure<FormOptions>(options =>
{
    // Tek seferde kabul edilecek maximum "multipart" boyutu (örneğin 1 GB)
    options.MultipartBodyLengthLimit = 1024L * 1024L * 1024L; 

    // Gerekirse şu limitleri de artırabilirsiniz:
    // options.ValueLengthLimit = int.MaxValue;
    // options.MultipartHeadersLengthLimit = int.MaxValue;
});

// PostgreSQL bağlantısını ekle
builder.Services.AddDbContext<MobSecLabContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Authentication (Cookie)
builder.Services.AddAuthentication("CookieAuth")
    .AddCookie("CookieAuth", options =>
    {
        options.LoginPath = "/Auth/Login";
    });

// Session ekliyoruz
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// IHttpClientFactory için
builder.Services.AddHttpClient();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Authentication & Authorization
app.UseAuthentication();
app.UseAuthorization();

// Session middleware
app.UseSession();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();