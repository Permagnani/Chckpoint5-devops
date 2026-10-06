using DimDim.Web.Data;
using DimDim.Web.Models;
using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DimDim.Web.Controllers;

public class ClientesController : Controller
{
    private readonly DimDimContext _db;
    private readonly TelemetryClient _telemetry;
    private readonly ILogger<ClientesController> _logger;

    public ClientesController(DimDimContext db, TelemetryClient telemetry, ILogger<ClientesController> logger)
    {
        _db = db;
        _telemetry = telemetry;
        _logger = logger;
    }

    // READ (lista)
    public async Task<IActionResult> Index(string? busca)
    {
        var query = _db.Clientes.Include(c => c.Contas).AsQueryable();
        if (!string.IsNullOrWhiteSpace(busca))
            query = query.Where(c => c.Nome.Contains(busca) || c.Cpf.Contains(busca));

        ViewData["Busca"] = busca;
        return View(await query.OrderBy(c => c.Nome).ToListAsync());
    }

    // READ (detalhe)
    public async Task<IActionResult> Details(int id)
    {
        var cliente = await _db.Clientes.Include(c => c.Contas)
            .FirstOrDefaultAsync(c => c.Id == id);
        return cliente is null ? NotFound() : View(cliente);
    }

    // CREATE
    public IActionResult Create() => View(new Cliente());

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Nome,Cpf,Email,Telefone")] Cliente cliente)
    {
        if (await _db.Clientes.AnyAsync(c => c.Cpf == cliente.Cpf))
            ModelState.AddModelError(nameof(Cliente.Cpf), "Já existe um cliente com esse CPF.");

        if (!ModelState.IsValid) return View(cliente);

        cliente.DataCadastro = DateTime.UtcNow;
        _db.Clientes.Add(cliente);
        await _db.SaveChangesAsync();

        Registrar("ClienteCriado", cliente.Id);
        TempData["Sucesso"] = $"Cliente {cliente.Nome} cadastrado (Id {cliente.Id}).";
        return RedirectToAction(nameof(Index));
    }

    // UPDATE
    public async Task<IActionResult> Edit(int id)
    {
        var cliente = await _db.Clientes.FindAsync(id);
        return cliente is null ? NotFound() : View(cliente);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,Nome,Cpf,Email,Telefone")] Cliente form)
    {
        if (id != form.Id) return BadRequest();

        if (await _db.Clientes.AnyAsync(c => c.Cpf == form.Cpf && c.Id != id))
            ModelState.AddModelError(nameof(Cliente.Cpf), "Já existe outro cliente com esse CPF.");

        if (!ModelState.IsValid) return View(form);

        var cliente = await _db.Clientes.FindAsync(id);
        if (cliente is null) return NotFound();

        cliente.Nome = form.Nome;
        cliente.Cpf = form.Cpf;
        cliente.Email = form.Email;
        cliente.Telefone = form.Telefone;
        await _db.SaveChangesAsync();

        Registrar("ClienteAtualizado", cliente.Id);
        TempData["Sucesso"] = $"Cliente {cliente.Nome} atualizado.";
        return RedirectToAction(nameof(Index));
    }

    // DELETE
    public async Task<IActionResult> Delete(int id)
    {
        var cliente = await _db.Clientes.Include(c => c.Contas)
            .FirstOrDefaultAsync(c => c.Id == id);
        return cliente is null ? NotFound() : View(cliente);
    }

    [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var cliente = await _db.Clientes.FindAsync(id);
        if (cliente is null) return NotFound();

        _db.Clientes.Remove(cliente); // contas do cliente são removidas em cascata (FK)
        await _db.SaveChangesAsync();

        Registrar("ClienteExcluido", id);
        TempData["Sucesso"] = $"Cliente {cliente.Nome} excluído, junto com as contas dele.";
        return RedirectToAction(nameof(Index));
    }

    private void Registrar(string evento, int id)
    {
        _telemetry.TrackEvent(evento, new Dictionary<string, string> { ["ClienteId"] = id.ToString() });
        _logger.LogInformation("{Evento}: cliente {ClienteId}", evento, id);
    }
}
