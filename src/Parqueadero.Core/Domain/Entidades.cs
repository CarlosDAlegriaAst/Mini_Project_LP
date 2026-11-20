namespace Parqueadero.Core.Domain;

public enum TipoVehiculo { Carro = 1, Moto = 2, Bicicleta = 3 }

/// <summary>Clase abstracta: comparte la clave primaria de todas las entidades (herencia).</summary>
public abstract class EntidadBase
{
    public int Id { get; set; }
}

public class Espacio : EntidadBase
{
    public string Codigo { get; set; } = "";
    public TipoVehiculo Tipo { get; set; }
    public bool Ocupado { get; set; }
}

public class Tarifa : EntidadBase
{
    public TipoVehiculo Tipo { get; set; }
    public decimal ValorHora { get; set; }
}

public class Registro : EntidadBase
{
    public string Placa { get; set; } = "";
    public TipoVehiculo Tipo { get; set; }
    public int EspacioId { get; set; }
    public Espacio? Espacio { get; set; }
    public DateTime Entrada { get; set; }
    public DateTime? Salida { get; set; }
    public decimal? ValorCobrado { get; set; }
    public bool Activo => Salida is null;
}

/// <summary>record inmutable: resultado de una operación (evita excepciones en flujos normales).</summary>
public record ResultadoOperacion(bool Exito, string Mensaje, Registro? Registro = null);

/// <summary>record para reportes por tipo de vehículo.</summary>
public record ResumenTipo(TipoVehiculo Tipo, int Cantidad, decimal Total);
