using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.StackExchangeRedis; // ← Redis
using PortalInmobiliario.Data;
using PortalInmobiliario.Services; // ← CatalogoCache / AgendaService

var builder = WebApplication.CreateBuilder(args);

// --- DB e Identity ---
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// ⬇️ Habilitar Roles (AddRoles<IdentityRole>())
builder.Services.AddDefaultIdentity<IdentityUser>(opts =>
{
    opts.SignIn.RequireConfirmedAccount = false;
})
.AddRoles<IdentityRole>() // ← P5
.AddEntityFrameworkStores<ApplicationDbContext>();

// ⬇️ AccessDenied claro
builder.Services.ConfigureApplicationCookie(opt =>
{
    opt.AccessDeniedPath = "/Account/AccessDenied";
});

builder.Services.AddControllersWithViews();

// --- Redis Cache ---
builder.Services.AddStackExchangeRedisCache(opt =>
{
    opt.Configuration = builder.Configuration["Redis:ConnectionString"];
});

// --- Sesión ---
builder.Services.AddSession(o =>
{
    o.IdleTimeout = TimeSpan.FromMinutes(20);
    o.Cookie.HttpOnly = true;
    o.Cookie.IsEssential = true;
});

// --- Servicios P3/P4 ---
builder.Services.AddScoped<AgendaService>();
builder.Services.AddScoped<CatalogoCache>();

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

app.UseSession();

// ⬇️ Seed del rol "Broker" si no existe (P5)
using (var scope = app.Services.CreateScope())
{
    var roleMgr = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    if (!await roleMgr.RoleExistsAsync("Broker"))
        await roleMgr.CreateAsync(new IdentityRole("Broker"));
}

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

app.Run();
