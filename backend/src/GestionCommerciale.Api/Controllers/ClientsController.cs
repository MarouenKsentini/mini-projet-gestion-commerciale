using GestionCommerciale.Application.DTOs;
using GestionCommerciale.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace GestionCommerciale.Api.Controllers;

[ApiController]
[Route("api/clients")]
public class ClientsController : ControllerBase
{
    private readonly IClientService _service;

    public ClientsController(IClientService service) => _service = service;

    /// <summary>Liste tous les clients.</summary>
    [HttpGet]
    public async Task<ActionResult<List<ClientDto>>> GetAll()
        => Ok(await _service.GetAllAsync());

    /// <summary>Détail d'un client.</summary>
    [HttpGet("{id:int}")]
    public async Task<ActionResult<ClientDto>> GetById(int id)
        => Ok(await _service.GetByIdAsync(id));

    /// <summary>Crée un nouveau client.</summary>
    [HttpPost]
    public async Task<ActionResult<ClientDto>> Create([FromBody] ClientUpsertDto dto)
    {
        var created = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    /// <summary>Modifie un client existant.</summary>
    [HttpPut("{id:int}")]
    public async Task<ActionResult<ClientDto>> Update(int id, [FromBody] ClientUpsertDto dto)
        => Ok(await _service.UpdateAsync(id, dto));

    /// <summary>Supprime un client.</summary>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _service.DeleteAsync(id);
        return NoContent();
    }
}
