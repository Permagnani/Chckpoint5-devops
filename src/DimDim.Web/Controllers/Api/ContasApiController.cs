using DimDim.Web.Data;
using DimDim.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DimDim.Web.Controllers.Api;

[ApiController]
[Route("api/contas")]
public class ContasApiController : ControllerBase
{
    private readonly DimDimContext _db;
    public ContasApiController(DimDimContext db) => _db = db;

    [HttpGet]
    public async Task<IEnumerable<Conta>> Get() =>
        await _db.Contas.AsNoTracking().OrderBy(c => c.Id).ToListAsync();

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Conta>> Get(int id)
    {
        var conta = await _db.Contas.AsNoTracking().Include(c => c.Cliente)
            .FirstOrDefaultAsync(c => c.Id == id);
        return conta is null ? NotFound() : conta;
    }

    [HttpPost]
    public async Task<ActionResult<Conta>> Post(Conta conta)
    {
        if (!await _db.Clientes.AnyAsync(c => c.Id == conta.ClienteId))
            return BadRequest(new { erro = "ClienteId inexistente." });
        if (await _db.Contas.AnyAsync(c => c.Numero == conta.Numero))
            return Conflict(new { erro = "Número de conta já existe." });

        conta.Id = 0;
        conta.Cliente = null;
        conta.DataAbertura = DateTime.UtcNow;
        _db.Contas.Add(conta);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(Get), new { id = conta.Id }, conta);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Put(int id, Conta form)
    {
        var conta = await _db.Contas.FindAsync(id);
        if (conta is null) return NotFound();
        if (!await _db.Clientes.AnyAsync(c => c.Id == form.ClienteId))
            return BadRequest(new { erro = "ClienteId inexistente." });
        if (await _db.Contas.AnyAsync(c => c.Numero == form.Numero && c.Id != id))
            return Conflict(new { erro = "Número de conta já existe." });

        conta.Numero = form.Numero;
        conta.Agencia = form.Agencia;
        conta.Tipo = form.Tipo;
        conta.Saldo = form.Saldo;
        conta.ClienteId = form.ClienteId;
        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var conta = await _db.Contas.FindAsync(id);
        if (conta is null) return NotFound();
        _db.Contas.Remove(conta);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
