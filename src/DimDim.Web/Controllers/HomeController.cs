using DimDim.Web.Data;
using DimDim.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DimDim.Web.Controllers;

public class HomeController : Controller
{
    private readonly DimDimContext _db;

    public HomeController(DimDimContext db) => _db = db;

    public async Task<IActionResult> Index()
    {
        var vm = new DashboardViewModel
        {
            TotalClientes = await _db.Clientes.CountAsync(),
            TotalContas = await _db.Contas.CountAsync(),
            SaldoTotal = await _db.Contas.SumAsync(c => (decimal?)c.Saldo) ?? 0m,
            UltimasContas = await _db.Contas.Include(c => c.Cliente)
                .OrderByDescending(c => c.DataAbertura).Take(5).ToListAsync()
        };
        return View(vm);
    }

    public IActionResult Error() => View();
}
