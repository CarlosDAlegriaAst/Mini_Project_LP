using Parqueadero.Core.Domain;
namespace Parqueadero.Core.Services;

/// <summary>Polimorfismo: cada tipo de vehículo se cobra con su propia regla.</summary>
public abstract class EstrategiaCobro
{
    protected abstract int MinutosPorFraccion { get; }

    public decimal Calcular(TimeSpan tiempo, decimal valorHora)
    {
        var fracciones = Math.Max(1, (int)Math.Ceiling(tiempo.TotalMinutes / MinutosPorFraccion));
        return fracciones * valorHora * MinutosPorFraccion / 60m;
    }

    public static EstrategiaCobro Para(TipoVehiculo tipo) => tipo switch
    {
        TipoVehiculo.Bicicleta => new CobroMediaHora(),
        _ => new CobroHora()
    };
}

public sealed class CobroHora : EstrategiaCobro { protected override int MinutosPorFraccion => 60; }
public sealed class CobroMediaHora : EstrategiaCobro { protected override int MinutosPorFraccion => 30; }
