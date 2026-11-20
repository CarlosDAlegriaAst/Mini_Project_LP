using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Parqueadero.Core.Interfaces;
using Parqueadero.Core.Services;
using Parqueadero.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(o => o.UseSqlite($"Data Source={AppDbContext.RutaDb}"));
Directory.CreateDirectory(Path.GetDirectoryName(AppDbContext.RutaDb)!);

builder.Services.AddDefaultIdentity<IdentityUser>(o => o.SignIn.RequireConfirmedAccount = false)
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<AppDbContext>();

builder.Services.AddScoped<IParqueaderoRepository, EfParqueaderoRepository>();
builder.Services.AddScoped<ParqueaderoService>();
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    DbSeeder.SeedDatos(db);
    DbSeeder.SeedUsuarios(db);
}

app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllerRoute("default", "{controller=Parqueadero}/{action=Index}/{id?}");
app.MapRazorPages();
app.Run();
