using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Parqueadero.Data;
using Microsoft.EntityFrameworkCore;
using Parqueadero.Web.Models;

namespace Parqueadero.Web.Controllers;

[Authorize(Roles = "Admin")]
public class TarifasController : Controller
{
    private readonly AppDbContext _db;
    public TarifasController(AppDbContext db) => _db = db;

    public async Task<IActionResult> Index() => View(await _db.Tarifas.ToListAsync());

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Guardar(TarifaVM vm)
    {
        if (!ModelState.IsValid)
            TempData["error"] = string.Join(" ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
        else if (await _db.Tarifas.FindAsync(vm.Id) is { } t)
        {
            t.ValorHora = vm.ValorHora;
            await _db.SaveChangesAsync();
            TempData["ok"] = "Tarifa actualizada.";
        }
        return RedirectToAction(nameof(Index));
    }
}
