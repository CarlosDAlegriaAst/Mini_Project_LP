using System.ComponentModel.DataAnnotations;
using Parqueadero.Core.Domain;
namespace Parqueadero.Web.Models;

public class IngresoVM
{
    [Required(ErrorMessage = "La placa es obligatoria.")]
    [StringLength(8, MinimumLength = 3, ErrorMessage = "La placa debe tener entre 3 y 8 caracteres.")]
    [RegularExpression(@"^[A-Za-z0-9]+$", ErrorMessage = "Use solo letras y números, sin espacios.")]
    public string Placa { get; set; } = "";

    [Required, EnumDataType(typeof(TipoVehiculo), ErrorMessage = "Tipo de vehículo inválido.")]
    public TipoVehiculo Tipo { get; set; }
}

public class SalidaVM
{
    [Required(ErrorMessage = "La placa es obligatoria.")]
    [StringLength(8, MinimumLength = 3)]
    [RegularExpression(@"^[A-Za-z0-9]+$", ErrorMessage = "Use solo letras y números, sin espacios.")]
    public string Placa { get; set; } = "";
}

public class TarifaVM
{
    [Required] public int Id { get; set; }
    [Required, Range(0, 1_000_000, ErrorMessage = "La tarifa debe estar entre 0 y 1.000.000.")]
    public decimal ValorHora { get; set; }
}
