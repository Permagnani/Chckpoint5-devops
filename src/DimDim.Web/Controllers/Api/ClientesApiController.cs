using DimDim.Web.Data;
using DimDim.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DimDim.Web.Controllers.Api;

// Endpoints REST complementares ao front end (exemplos em api/operacoes.json)
[ApiController]
[Route("api/clientes")]
public class ClientesApiController : ControllerBase
{
    private readonly DimDimContext _db;
    public ClientesApiController(DimDimContext db) => _db = db;

    [HttpGet]
    public async Task<IEnumerable<Cliente>> Get() =>
        await _db.Clientes.AsNoTracking().OrderBy(c => c.Id).ToListAsync();

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Cliente>> Get(int id)
    {
        var cliente = await _db.Clientes.AsNoTracking().Include(c => c.Contas)
            .FirstOrDefaultAsync(c => c.Id == id);
        return cliente is null ? NotFound() : cliente;
    }

    [HttpPost]
    public async Task<ActionResult<Cliente>> Post(Cliente cliente)
    {
        if (await _db.Clientes.AnyAsync(c => c.Cpf == cliente.Cpf))
            return Conflict(new { erro = "CPF já cadastrado." });

        cliente.Id = 0;
        cliente.DataCadastro = DateTime.UtcNow;
        cliente.Contas = new List<Conta>();
        _db.Clientes.Add(cliente);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(Get), new { id = cliente.Id }, cliente);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Put(int id, Cliente form)
    {
        var cliente = await _db.Clientes.FindAsync(id);
        if (cliente is null) return NotFound();

        if (await _db.Clientes.AnyAsync(c => c.Cpf == form.Cpf && c.Id != id))
            return Conflict(new { erro = "CPF já cadastrado em outro cliente." });

        cliente.Nome = form.Nome;
        cliente.Cpf = form.Cpf;
        cliente.Email = form.Email;
        cliente.Telefone = form.Telefone;
        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var cliente = await _db.Clientes.FindAsync(id);
        if (cliente is null) return NotFound();
        _db.Clientes.Remove(cliente);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
