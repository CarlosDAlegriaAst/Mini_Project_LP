using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Parqueadero.Core.Domain;
using Parqueadero.Core.Interfaces;
using Parqueadero.Core.Services;
using Parqueadero.Web.Models;

namespace Parqueadero.Web.Controllers;

public class PanelVM
{
    public Dictionary<TipoVehiculo, (int Ocupados, int Total)> Ocupacion { get; set; } = new();
    public List<Registro> Activos { get; set; } = new();
    public IReadOnlyCollection<string> Movimientos { get; set; } = Array.Empty<string>();
    public int EnEspera { get; set; }
}

[Authorize(Roles = "Admin,Operador")]
public class ParqueaderoController : Controller
{
    private readonly ParqueaderoService _svc;
    private readonly IParqueaderoRepository _repo;
    public ParqueaderoController(ParqueaderoService svc, IParqueaderoRepository repo) { _svc = svc; _repo = repo; }

    public async Task<IActionResult> Index()
    {
        var vm = new PanelVM
        {
            Ocupacion = await _svc.OcupacionAsync(),
            Activos = (await _repo.HistorialAsync()).Filtrar(r => r.Activo).ToList(),
            Movimientos = _svc.UltimosMovimientos,
            EnEspera = _svc.EnEspera
        };
        return View(vm);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Ingreso(IngresoVM vm)
    {
        if (!ModelState.IsValid) return Error();
        var r = await _svc.IngresarAsync(vm.Placa, vm.Tipo);
        TempData[r.Exito ? "ok" : "error"] = r.Mensaje;
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Salida(SalidaVM vm)
    {
        if (!ModelState.IsValid) return Error();
        var r = await _svc.SalirAsync(vm.Placa);
        TempData[r.Exito ? "ok" : "error"] = r.Mensaje;
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Historial(int pagina = 1)
    {
        const int tamano = 20;
        var todos = (await _repo.HistorialAsync()).Filtrar(r => !r.Activo).ToList();
        ViewBag.Resumen = await _svc.ResumenAsync();
        ViewBag.Pagina = Math.Max(1, pagina);
        ViewBag.HayMas = todos.Count > ViewBag.Pagina * tamano;
        return View(todos.Pagina(pagina, tamano));
    }

    private IActionResult Error()
    {
        TempData["error"] = string.Join(" ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
        return RedirectToAction(nameof(Index));
    }
}
