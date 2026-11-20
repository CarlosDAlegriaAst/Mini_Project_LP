namespace Parqueadero.Core.Domain;

/// <summary>Excepción base del dominio.</summary>
public class ParqueaderoException : Exception
{
    public ParqueaderoException(string mensaje) : base(mensaje) { }
}

public class PlacaInvalidaException : ParqueaderoException
{
    public PlacaInvalidaException(string placa, TipoVehiculo tipo)
        : base($"La placa '{placa}' no es válida para {tipo}. " + ValidadorPlaca.Formato(tipo)) { }
}
