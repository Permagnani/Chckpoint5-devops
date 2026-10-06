using DimDim.Web.Data;
using DimDim.Web.Models;
using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace DimDim.Web.Controllers;

public class ContasController : Controller
{
    private readonly DimDimContext _db;
    private readonly TelemetryClient _telemetry;
    private readonly ILogger<ContasController> _logger;

    public ContasController(DimDimContext db, TelemetryClient telemetry, ILogger<ContasController> logger)
    {
        _db = db;
        _telemetry = telemetry;
        _logger = logger;
    }

    // READ (lista)
    public async Task<IActionResult> Index(int? clienteId)
    {
        var query = _db.Contas.Include(c => c.Cliente).AsQueryable();
        if (clienteId.HasValue) query = query.Where(c => c.ClienteId == clienteId);

        ViewData["ClienteId"] = clienteId;
        return View(await query.OrderBy(c => c.Numero).ToListAsync());
    }

    // READ (detalhe)
    public async Task<IActionResult> Details(int id)
    {
        var conta = await _db.Contas.Include(c => c.Cliente).FirstOrDefaultAsync(c => c.Id == id);
        return conta is null ? NotFound() : View(conta);
    }

    // CREATE
    public async Task<IActionResult> Create(int? clienteId)
    {
        if (!await _db.Clientes.AnyAsync())
        {
            TempData["Aviso"] = "Cadastre um cliente antes de abrir uma conta.";
            return RedirectToAction("Create", "Clientes");
        }
        await CarregarClientes(clienteId);
        return View(new Conta { ClienteId = clienteId ?? 0, Numero = GerarNumero() });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Numero,Agencia,Tipo,Saldo,ClienteId")] Conta conta)
    {
        await ValidarConta(conta);
        if (!ModelState.IsValid)
        {
            await CarregarClientes(conta.ClienteId);
            return View(conta);
        }

        conta.DataAbertura = DateTime.UtcNow;
        _db.Contas.Add(conta);
        await _db.SaveChangesAsync();

        Registrar("ContaCriada", conta.Id, conta.ClienteId);
        TempData["Sucesso"] = $"Conta {conta.Numero} aberta (Id {conta.Id}).";
        return RedirectToAction(nameof(Index));
    }

    // UPDATE
    public async Task<IActionResult> Edit(int id)
    {
        var conta = await _db.Contas.FindAsync(id);
        if (conta is null) return NotFound();
        await CarregarClientes(conta.ClienteId);
        return View(conta);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,Numero,Agencia,Tipo,Saldo,ClienteId")] Conta form)
    {
        if (id != form.Id) return BadRequest();

        await ValidarConta(form, id);
        if (!ModelState.IsValid)
        {
            await CarregarClientes(form.ClienteId);
            return View(form);
        }

        var conta = await _db.Contas.FindAsync(id);
        if (conta is null) return NotFound();

        conta.Numero = form.Numero;
        conta.Agencia = form.Agencia;
        conta.Tipo = form.Tipo;
        conta.Saldo = form.Saldo;
        conta.ClienteId = form.ClienteId;
        await _db.SaveChangesAsync();

        Registrar("ContaAtualizada", conta.Id, conta.ClienteId);
        TempData["Sucesso"] = $"Conta {conta.Numero} atualizada.";
        return RedirectToAction(nameof(Index));
    }

    // DELETE
    public async Task<IActionResult> Delete(int id)
    {
        var conta = await _db.Contas.Include(c => c.Cliente).FirstOrDefaultAsync(c => c.Id == id);
        return conta is null ? NotFound() : View(conta);
    }

    [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var conta = await _db.Contas.FindAsync(id);
        if (conta is null) return NotFound();

        _db.Contas.Remove(conta);
        await _db.SaveChangesAsync();

        Registrar("ContaExcluida", id, conta.ClienteId);
        TempData["Sucesso"] = $"Conta {conta.Numero} encerrada.";
        return RedirectToAction(nameof(Index));
    }

    private async Task ValidarConta(Conta conta, int? idAtual = null)
    {
        if (!await _db.Clientes.AnyAsync(c => c.Id == conta.ClienteId))
            ModelState.AddModelError(nameof(Conta.ClienteId), "Escolha um cliente existente.");

        var id = idAtual ?? 0;
        if (await _db.Contas.AnyAsync(c => c.Numero == conta.Numero && c.Id != id))
            ModelState.AddModelError(nameof(Conta.Numero), "Já existe uma conta com esse número.");
    }

    private async Task CarregarClientes(int? selecionado)
    {
        var clientes = await _db.Clientes.OrderBy(c => c.Nome)
            .Select(c => new { c.Id, Texto = c.Nome + " (" + c.Cpf + ")" })
            .ToListAsync();
        ViewBag.Clientes = new SelectList(clientes, "Id", "Texto", selecionado);
    }

    private static string GerarNumero() =>
        $"{Random.Shared.Next(10000, 99999)}-{Random.Shared.Next(0, 9)}";

    private void Registrar(string evento, int contaId, int clienteId)
    {
        _telemetry.TrackEvent(evento, new Dictionary<string, string>
        {
            ["ContaId"] = contaId.ToString(),
            ["ClienteId"] = clienteId.ToString()
        });
        _logger.LogInformation("{Evento}: conta {ContaId} do cliente {ClienteId}", evento, contaId, clienteId);
    }
}
