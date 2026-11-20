using System.Text.RegularExpressions;
using Parqueadero.Core.Domain;
using Parqueadero.Core.Services;
namespace Parqueadero.Core;

/// <summary>Validación de placas colombianas con expresiones regulares.</summary>
public static class ValidadorPlaca
{
    private static readonly Regex Carro = new(@"^[A-Z]{3}\d{3}$", RegexOptions.Compiled);
    private static readonly Regex Moto = new(@"^[A-Z]{3}\d{2}[A-Z]$", RegexOptions.Compiled);
    private static readonly Regex Bici = new(@"^[A-Z0-9]{3,8}$", RegexOptions.Compiled);

    public static string Formato(TipoVehiculo tipo) => tipo switch   // switch expression
    {
        TipoVehiculo.Carro => "Formato: 3 letras y 3 números (ABC123).",
        TipoVehiculo.Moto => "Formato: 3 letras, 2 números y 1 letra (ABC12D).",
        _ => "Formato: entre 3 y 8 letras o números."
    };

    public static bool EsValida(string? placa, TipoVehiculo tipo)
    {
        if (string.IsNullOrWhiteSpace(placa)) return false;
        var p = placa.Normalizar();
        return tipo switch
        {
            TipoVehiculo.Carro => Carro.IsMatch(p),
            TipoVehiculo.Moto => Moto.IsMatch(p),
            _ => Bici.IsMatch(p)
        };
    }
}
