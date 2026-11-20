using Parqueadero.Core.Domain;
using Parqueadero.Core.Services;
using Parqueadero.Data;
namespace Parqueadero.Desktop;

public class MainForm : Form
{
    private readonly AppDbContext _db = new(AppDbContext.Opciones());
    private readonly ParqueaderoService _svc;
    private readonly TextBox _placa = new() { PlaceholderText = "Placa", Width = 120 };
    private readonly ComboBox _tipo = new() { DropDownStyle = ComboBoxStyle.DropDownList, Width = 110 };
    private readonly DataGridView _grid = new() { Dock = DockStyle.Fill, ReadOnly = true, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, AllowUserToAddRows = false };
    private readonly Label _estado = new() { Dock = DockStyle.Bottom, Height = 28 };
    private readonly bool _esAdmin;

    public MainForm(string usuario, IList<string> roles)
    {
        _svc = new ParqueaderoService(new EfParqueaderoRepository(_db));
        _esAdmin = roles.Contains(DbSeeder.RolAdmin);
        Text = $"Parqueadero - {usuario} ({string.Join(", ", roles)})";
        Size = new Size(820, 520);
        _tipo.DataSource = Enum.GetValues<TipoVehiculo>();

        var barra = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 44, Padding = new Padding(6) };
        var entrada = new Button { Text = "Entrada", Width = 90 };
        var salida = new Button { Text = "Salida y cobro", Width = 120 };
        var recaudo = new Button { Text = "Recaudo (Admin)", Width = 130, Enabled = _esAdmin };
        entrada.Click += async (_, _) => Mostrar(await _svc.IngresarAsync(_placa.Text, (TipoVehiculo)_tipo.SelectedItem!));
        salida.Click += async (_, _) => Mostrar(await _svc.SalirAsync(_placa.Text));
        recaudo.Click += async (_, _) =>
        {
            var r = await _svc.ResumenAsync();
            MessageBox.Show(r.Count == 0 ? "Aún no hay salidas." :
                string.Join("\n", r.Select(x => $"{x.Tipo}: {x.Cantidad} salidas, ${x.Total:N0}")), "Recaudo");
        };
        barra.Controls.AddRange(new Control[] { _placa, _tipo, entrada, salida, recaudo });
        Controls.Add(_grid); Controls.Add(barra); Controls.Add(_estado);

        // Suscripción a eventos del dominio
        _svc.VehiculoIngresado += r => _estado.Text = $"Ingresó {r.Placa}";
        _svc.VehiculoSalio += r => _estado.Text = $"Salió {r.Placa}";
        Load += async (_, _) => await RefrescarAsync();
    }

    private async void Mostrar(ResultadoOperacion r)
    {
        _estado.Text = r.Mensaje;
        _estado.ForeColor = r.Exito ? Color.DarkGreen : Color.Firebrick;
        await RefrescarAsync();
    }

    private async Task RefrescarAsync()
    {
        var activos = (await new EfParqueaderoRepository(_db).HistorialAsync()).Where(r => r.Activo)
            .Select(r => new { r.Placa, r.Tipo, Espacio = r.Espacio?.Codigo, Entrada = r.Entrada.ToString("dd/MM HH:mm") }).ToList();
        _grid.DataSource = activos;
    }
}
