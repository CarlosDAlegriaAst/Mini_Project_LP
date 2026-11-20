using Parqueadero.Core;
using Parqueadero.Core.Domain;
using Parqueadero.Core.Interfaces;
using Parqueadero.Core.Services;
using Xunit;
namespace Parqueadero.Tests;

/// <summary>Repositorio en memoria: la lógica de negocio se prueba sin base de datos.</summary>
class RepoFalso : IParqueaderoRepository
{
    public List<Espacio> Espacios = new() { new() { Id = 1, Codigo = "C-01", Tipo = TipoVehiculo.Carro } };
    public List<Registro> Registros = new();
    public Task<List<Espacio>> EspaciosAsync() => Task.FromResult(Espacios);
    public Task<Espacio?> EspacioLibreAsync(TipoVehiculo t) => Task.FromResult(Espacios.FirstOrDefault(e => e.Tipo == t && !e.Ocupado));
    public Task<Registro?> ActivoPorPlacaAsync(string p) => Task.FromResult(Registros.FirstOrDefault(r => r.Placa == p && r.Activo));
    public Task<List<Registro>> HistorialAsync() => Task.FromResult(Registros);
    public Task<List<Tarifa>> TarifasAsync() => Task.FromResult(new List<Tarifa>());
    public Task<Tarifa?> TarifaAsync(TipoVehiculo t) => Task.FromResult<Tarifa?>(new Tarifa { Tipo = t, ValorHora = 4000 });
    public Task AgregarAsync(Registro r) { r.Espacio = Espacios.First(e => e.Id == r.EspacioId); Registros.Add(r); return Task.CompletedTask; }
    public Task GuardarAsync() => Task.CompletedTask;
}

public class ServicioTests
{
    [Theory]
    [InlineData("ABC123", TipoVehiculo.Carro, true)]
    [InlineData("abc12d", TipoVehiculo.Moto, true)]
    [InlineData("AB1234", TipoVehiculo.Carro, false)]
    [InlineData("", TipoVehiculo.Carro, false)]
    public void ValidaPlacas(string placa, TipoVehiculo tipo, bool esperado) =>
        Assert.Equal(esperado, ValidadorPlaca.EsValida(placa, tipo));

    [Fact]
    public void Cobro_Carro_CobraPorHoraOFraccion() =>
        Assert.Equal(8000m, ParqueaderoService.CalcularCobro(DateTime.Today, DateTime.Today.AddMinutes(61), 4000, TipoVehiculo.Carro));

    [Fact]
    public void Cobro_Bicicleta_CobraPorMediaHora() =>
        Assert.Equal(250m, ParqueaderoService.CalcularCobro(DateTime.Today, DateTime.Today.AddMinutes(10), 500, TipoVehiculo.Bicicleta));

    [Fact]
    public async Task Ingreso_PlacaInvalida_Rechaza()
    {
        var r = await new ParqueaderoService(new RepoFalso()).IngresarAsync("123", TipoVehiculo.Carro);
        Assert.False(r.Exito);
    }

    [Fact]
    public async Task Ingreso_Duplicado_Rechaza()
    {
        var s = new ParqueaderoService(new RepoFalso());
        Assert.True((await s.IngresarAsync("DUP123", TipoVehiculo.Carro)).Exito);
        Assert.False((await s.IngresarAsync("DUP123", TipoVehiculo.Carro)).Exito);
    }

    [Fact]
    public async Task Salida_LiberaEspacioYAtiendeEspera()
    {
        var repo = new RepoFalso();
        var s = new ParqueaderoService(repo);
        await s.IngresarAsync("AAA111", TipoVehiculo.Carro);
        var espera = await s.IngresarAsync("BBB222", TipoVehiculo.Carro);   // sin cupo → cola
        Assert.False(espera.Exito);
        var salida = await s.SalirAsync("AAA111");
        Assert.True(salida.Exito);
        Assert.NotNull(await repo.ActivoPorPlacaAsync("BBB222"));            // entró desde la cola
    }
}
