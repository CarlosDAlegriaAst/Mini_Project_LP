using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Parqueadero.Core.Domain;
using Parqueadero.Core.Interfaces;
namespace Parqueadero.Data;

public class AppDbContext : IdentityDbContext<IdentityUser>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
    public DbSet<Espacio> Espacios => Set<Espacio>();
    public DbSet<Tarifa> Tarifas => Set<Tarifa>();
    public DbSet<Registro> Registros => Set<Registro>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        base.OnModelCreating(b);
        b.Entity<Registro>().HasIndex(r => r.Placa);
        b.Entity<Registro>().Property(r => r.ValorCobrado).HasPrecision(18, 2);
        b.Entity<Registro>().Ignore(r => r.Activo);
        b.Entity<Tarifa>().Property(t => t.ValorHora).HasPrecision(18, 2);
    }

    /// <summary>La BD SQLite es compartida entre la web y el escritorio.</summary>
    public static string RutaDb => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Parqueadero", "parqueadero.db");

    public static DbContextOptions<AppDbContext> Opciones()
    {
        Directory.CreateDirectory(Path.GetDirectoryName(RutaDb)!);
        return new DbContextOptionsBuilder<AppDbContext>().UseSqlite($"Data Source={RutaDb}").Options;
    }
}

public class EfParqueaderoRepository : IParqueaderoRepository
{
    private readonly AppDbContext _db;
    public EfParqueaderoRepository(AppDbContext db) => _db = db;

    public Task<List<Espacio>> EspaciosAsync() => _db.Espacios.OrderBy(e => e.Codigo).ToListAsync();
    public Task<Espacio?> EspacioLibreAsync(TipoVehiculo t) => _db.Espacios.FirstOrDefaultAsync(e => e.Tipo == t && !e.Ocupado);
    public Task<Registro?> ActivoPorPlacaAsync(string p) =>
        _db.Registros.Include(r => r.Espacio).FirstOrDefaultAsync(r => r.Placa == p && r.Salida == null);
    public Task<List<Registro>> HistorialAsync() =>
        _db.Registros.Include(r => r.Espacio).OrderByDescending(r => r.Entrada).ToListAsync();
    public Task<List<Tarifa>> TarifasAsync() => _db.Tarifas.ToListAsync();
    public Task<Tarifa?> TarifaAsync(TipoVehiculo t) => _db.Tarifas.FirstOrDefaultAsync(x => x.Tipo == t);
    public async Task AgregarAsync(Registro r) => await _db.Registros.AddAsync(r);
    public Task GuardarAsync() => _db.SaveChangesAsync();
}

public static class DbSeeder
{
    public const string RolAdmin = "Admin", RolOperador = "Operador";

    public static void SeedDatos(AppDbContext db)
    {
        db.Database.EnsureCreated();
        if (!db.Tarifas.Any())
            db.Tarifas.AddRange(
                new Tarifa { Tipo = TipoVehiculo.Carro, ValorHora = 4000 },
                new Tarifa { Tipo = TipoVehiculo.Moto, ValorHora = 2000 },
                new Tarifa { Tipo = TipoVehiculo.Bicicleta, ValorHora = 500 });
        if (!db.Espacios.Any())
        {
            for (int i = 1; i <= 10; i++) db.Espacios.Add(new Espacio { Codigo = $"C-{i:00}", Tipo = TipoVehiculo.Carro });
            for (int i = 1; i <= 6; i++) db.Espacios.Add(new Espacio { Codigo = $"M-{i:00}", Tipo = TipoVehiculo.Moto });
            for (int i = 1; i <= 4; i++) db.Espacios.Add(new Espacio { Codigo = $"B-{i:00}", Tipo = TipoVehiculo.Bicicleta });
        }
        db.SaveChanges();
    }


    public static void SeedUsuarios(AppDbContext db)
    {
        foreach (var rol in new[] { RolAdmin, RolOperador })
            if (!db.Roles.Any(r => r.Name == rol))
                db.Roles.Add(new IdentityRole { Name = rol, NormalizedName = rol.ToUpperInvariant() });
        db.SaveChanges();

        Crear(db, "admin@parqueadero.com", "Admin123*", RolAdmin);
        Crear(db, "operador@parqueadero.com", "Operador123*", RolOperador);
    }

    private static void Crear(AppDbContext db, string email, string pass, string rol)
    {
        if (db.Users.Any(u => u.Email == email)) return;
        var u = new IdentityUser { UserName = email, NormalizedUserName = email.ToUpperInvariant(),
            Email = email, NormalizedEmail = email.ToUpperInvariant(), EmailConfirmed = true,
            SecurityStamp = Guid.NewGuid().ToString() };
        u.PasswordHash = new PasswordHasher<IdentityUser>().HashPassword(u, pass);
        db.Users.Add(u);
        var roleId = db.Roles.First(r => r.Name == rol).Id;
        db.UserRoles.Add(new IdentityUserRole<string> { UserId = u.Id, RoleId = roleId });
        db.SaveChanges();
    }

    /// <summary>Login del escritorio con el mismo hash de ASP.NET Core Identity.</summary>
    public static async Task<IList<string>?> ValidarAsync(AppDbContext db, string email, string password)
    {
        var user = await db.Users.FirstOrDefaultAsync(u => u.NormalizedEmail == email.Trim().ToUpperInvariant());
        if (user?.PasswordHash is null) return null;
        var ok = new PasswordHasher<IdentityUser>().VerifyHashedPassword(user, user.PasswordHash, password);
        if (ok == PasswordVerificationResult.Failed) return null;
        return await (from ur in db.UserRoles join r in db.Roles on ur.RoleId equals r.Id
                      where ur.UserId == user.Id select r.Name!).ToListAsync();
    }
}
