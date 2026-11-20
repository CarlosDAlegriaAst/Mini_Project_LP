using Parqueadero.Core.Domain;
using Parqueadero.Core.Interfaces;
namespace Parqueadero.Core.Services;

public static class Extensiones
{
    // Método de extensión
    public static string Normalizar(this string placa) => placa.Trim().ToUpperInvariant();
}

public class ParqueaderoService
{
    private readonly IParqueaderoRepository _repo;

    private static readonly object _lock = new();
    // Stack<T>: últimos movimientos (LIFO, el más reciente primero).
    private static readonly Stack<string> _ultimosMovimientos = new();
    // Queue<T>: lista de espera cuando no hay cupo (FIFO).
    private static readonly Queue<(string Placa, TipoVehiculo Tipo)> _espera = new();

    // Eventos con delegados
    public event Action<Registro>? VehiculoIngresado;
    public event Action<Registro>? VehiculoSalio;

    public ParqueaderoService(IParqueaderoRepository repo) => _repo = repo;

    public IReadOnlyCollection<string> UltimosMovimientos { get { lock (_lock) return _ultimosMovimientos.Take(10).ToList(); } }
    public int EnEspera { get { lock (_lock) return _espera.Count; } }

    public async Task<ResultadoOperacion> IngresarAsync(string placa, TipoVehiculo tipo)
    {
        try
        {
            if (!ValidadorPlaca.EsValida(placa, tipo))
                throw new PlacaInvalidaException(placa ?? "", tipo);
            placa = placa.Normalizar();

            if (await _repo.ActivoPorPlacaAsync(placa) is not null)
                return new(false, $"El vehículo {placa} ya está dentro.");

            var espacio = await _repo.EspacioLibreAsync(tipo);
            if (espacio is null)
            {
                int pos;
                lock (_lock)
                {
                    if (_espera.Any(e => e.Placa == placa))
                        return new(false, $"{placa} ya está en lista de espera.");
                    _espera.Enqueue((placa, tipo));
                    pos = _espera.Count;
                }
                return new(false, $"Sin cupo para {tipo}. {placa} quedó en lista de espera (#{pos}).");
            }

            espacio.Ocupado = true;
            var registro = new Registro { Placa = placa, Tipo = tipo, EspacioId = espacio.Id, Entrada = DateTime.Now };
            await _repo.AgregarAsync(registro);
            await _repo.GuardarAsync();

            lock (_lock) _ultimosMovimientos.Push($"{DateTime.Now:HH:mm} ENTRA {placa} en {espacio.Codigo}");
            VehiculoIngresado?.Invoke(registro);
            return new(true, $"{placa} asignado al espacio {espacio.Codigo}.", registro);
        }
        catch (ParqueaderoException ex)   // excepción propia del dominio
        {
            return new(false, ex.Message);
        }
    }

    public async Task<ResultadoOperacion> SalirAsync(string placa)
    {
        if (string.IsNullOrWhiteSpace(placa)) return new(false, "La placa es obligatoria.");
        placa = placa.Normalizar();
        var registro = await _repo.ActivoPorPlacaAsync(placa);
        if (registro is null)
            return new(false, $"No hay un vehículo activo con placa {placa}.");

        var tarifa = await _repo.TarifaAsync(registro.Tipo);
        registro.Salida = DateTime.Now;
        registro.ValorCobrado = CalcularCobro(registro.Entrada, registro.Salida.Value, tarifa?.ValorHora ?? 0, registro.Tipo);
        if (registro.Espacio is not null) registro.Espacio.Ocupado = false;
        await _repo.GuardarAsync();

        lock (_lock) _ultimosMovimientos.Push($"{DateTime.Now:HH:mm} SALE {placa} ${registro.ValorCobrado:N0}");
        VehiculoSalio?.Invoke(registro);

        var extra = await AtenderEsperaAsync(registro.Tipo);
        return new(true, $"Total a cobrar: ${registro.ValorCobrado:N0}{extra}", registro);
    }

    /// <summary>Al liberarse un cupo, entra el primero en la cola que sea del mismo tipo (FIFO).</summary>
    private async Task<string> AtenderEsperaAsync(TipoVehiculo tipo)
    {
        (string Placa, TipoVehiculo Tipo)? siguiente = null;
        lock (_lock)
        {
            var resto = new Queue<(string, TipoVehiculo)>();
            while (_espera.Count > 0)
            {
                var x = _espera.Dequeue();
                if (siguiente is null && x.Tipo == tipo) siguiente = x; else resto.Enqueue(x);
            }
            foreach (var x in resto) _espera.Enqueue(x);
        }
        if (siguiente is null) return "";
        var r = await IngresarAsync(siguiente.Value.Placa, tipo);
        return r.Exito ? $" · Entró de la lista de espera: {siguiente.Value.Placa}" : "";
    }

    /// <summary>Cobro por polimorfismo: la estrategia depende del tipo de vehículo.</summary>
    public static decimal CalcularCobro(DateTime entrada, DateTime salida, decimal valorHora, TipoVehiculo tipo) =>
        EstrategiaCobro.Para(tipo).Calcular(salida - entrada, valorHora);

    public async Task<List<ResumenTipo>> ResumenAsync()
    {
        var historial = await _repo.HistorialAsync();
        return historial.Filtrar(r => !r.Activo)       // iterador genérico propio
            .GroupBy(r => r.Tipo)                       // LINQ
            .Select(g => new ResumenTipo(g.Key, g.Count(), g.Sum(r => r.ValorCobrado ?? 0)))
            .OrderByDescending(r => r.Total).ToList();
    }

    /// <summary>Dictionary: ocupación por tipo con acceso O(1) por clave.</summary>
    public async Task<Dictionary<TipoVehiculo, (int Ocupados, int Total)>> OcupacionAsync()
    {
        var espacios = await _repo.EspaciosAsync();
        return espacios.GroupBy(e => e.Tipo)
            .ToDictionary(g => g.Key, g => (g.Count(e => e.Ocupado), g.Count()));
    }
}
