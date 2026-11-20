using Parqueadero.Core.Domain;
namespace Parqueadero.Core.Interfaces;

public interface IParqueaderoRepository
{
    Task<List<Espacio>> EspaciosAsync();
    Task<Espacio?> EspacioLibreAsync(TipoVehiculo tipo);
    Task<Registro?> ActivoPorPlacaAsync(string placa);
    Task<List<Registro>> HistorialAsync();
    Task<List<Tarifa>> TarifasAsync();
    Task<Tarifa?> TarifaAsync(TipoVehiculo tipo);
    Task AgregarAsync(Registro registro);
    Task GuardarAsync();
}
