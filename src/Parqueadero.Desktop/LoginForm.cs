using Parqueadero.Data;
namespace Parqueadero.Desktop;

public class LoginForm : Form
{
    private readonly TextBox _email = new() { Dock = DockStyle.Top, PlaceholderText = "Correo" };
    private readonly TextBox _pass = new() { Dock = DockStyle.Top, PlaceholderText = "Contraseña", UseSystemPasswordChar = true };
    private readonly Label _msg = new() { Dock = DockStyle.Top, ForeColor = Color.Firebrick, Height = 24 };
    public string Usuario { get; private set; } = "";
    public IList<string> Roles { get; private set; } = new List<string>();

    public LoginForm()
    {
        Text = "Parqueadero - Ingresar";
        Size = new Size(340, 220);
        StartPosition = FormStartPosition.CenterScreen;
        FormBorderStyle = FormBorderStyle.FixedDialog; MaximizeBox = false;
        var btn = new Button { Text = "Ingresar", Dock = DockStyle.Top, Height = 36 };
        btn.Click += async (_, _) => await IngresarAsync();
        AcceptButton = btn;
        Padding = new Padding(16);
        Controls.AddRange(new Control[] { btn, _msg, _pass, _email });
    }

    private async Task IngresarAsync()
    {
        using var db = new AppDbContext(AppDbContext.Opciones());
        var roles = await DbSeeder.ValidarAsync(db, _email.Text, _pass.Text);
        if (roles is null) { _msg.Text = "Correo o contraseña incorrectos."; return; }
        Usuario = _email.Text; Roles = roles; DialogResult = DialogResult.OK;
    }
}
