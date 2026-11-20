using Parqueadero.Data;
namespace Parqueadero.Desktop;

static class Program
{
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();
        using (var db = new AppDbContext(AppDbContext.Opciones()))
        {
            DbSeeder.SeedDatos(db);
            DbSeeder.SeedUsuarios(db);
        }
        var login = new LoginForm();
        if (login.ShowDialog() == DialogResult.OK)
            Application.Run(new MainForm(login.Usuario, login.Roles));
    }
}
