using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.StackExchangeRedis; // ← Redis
using PortalInmobiliario.Data;
using PortalInmobiliario.Services; // ← CatalogoCache / AgendaService

var builder = WebApplication.CreateBuilder(args);

// --- DB e Identity (lo que ya tenías) ---
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<IdentityUser>(opts =>
{
    opts.SignIn.RequireConfirmedAccount = false;
})
.AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddControllersWithViews();

// --- ⬇️ NUEVO: Redis Cache (leerá Redis:ConnectionString o REDIS__ConnectionString) ---
builder.Services.AddStackExchangeRedisCache(opt =>
{
    opt.Configuration = builder.Configuration["Redis:ConnectionString"];
});

// --- ⬇️ NUEVO: Sesión ---
builder.Services.AddSession(o =>
{
    o.IdleTimeout = TimeSpan.FromMinutes(20);
    o.Cookie.HttpOnly = true;
    o.Cookie.IsEssential = true;
});

// --- ⬇️ NUEVO: Servicios P3/P4 ---
builder.Services.AddScoped<AgendaService>();     // si ya existía, no pasa nada
builder.Services.AddScoped<CatalogoCache>();     // cache de catálogo (P4)

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.UseSession(); // ← ⬅️ IMPORTANTE: habilita sesión antes de mapear rutas

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

app.Run();
