using GestionCommerciale.Application.DTOs;
using GestionCommerciale.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace GestionCommerciale.Api.Controllers;

[ApiController]
[Route("api/orders")]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _service;

    public OrdersController(IOrderService service) => _service = service;

    /// <summary>Liste toutes les commandes.</summary>
    [HttpGet]
    public async Task<ActionResult<List<OrderDto>>> GetAll()
        => Ok(await _service.GetAllAsync());

    /// <summary>Détail d'une commande (avec ses lignes).</summary>
    [HttpGet("{id:int}")]
    public async Task<ActionResult<OrderDto>> GetById(int id)
        => Ok(await _service.GetByIdAsync(id));

    /// <summary>Crée une nouvelle commande avec ses lignes.</summary>
    [HttpPost]
    public async Task<ActionResult<OrderDto>> Create([FromBody] OrderUpsertDto dto)
    {
        var created = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    /// <summary>Modifie une commande existante (uniquement si elle est en brouillon).</summary>
    [HttpPut("{id:int}")]
    public async Task<ActionResult<OrderDto>> Update(int id, [FromBody] OrderUpsertDto dto)
        => Ok(await _service.UpdateAsync(id, dto));

    /// <summary>Supprime une commande (impossible si déjà validée).</summary>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _service.DeleteAsync(id);
        return NoContent();
    }

    /// <summary>Valide la commande : vérifie le stock puis le décrémente.</summary>
    [HttpPost("{id:int}/validate")]
    public async Task<ActionResult<OrderDto>> Validate(int id)
        => Ok(await _service.ValidateAsync(id));
}
