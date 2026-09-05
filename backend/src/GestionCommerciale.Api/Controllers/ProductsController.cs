using GestionCommerciale.Application.DTOs;
using GestionCommerciale.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace GestionCommerciale.Api.Controllers;

[ApiController]
[Route("api/products")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _service;

    public ProductsController(IProductService service) => _service = service;

    /// <summary>Liste tous les produits.</summary>
    [HttpGet]
    public async Task<ActionResult<List<ProductDto>>> GetAll()
        => Ok(await _service.GetAllAsync());

    /// <summary>Détail d'un produit.</summary>
    [HttpGet("{id:int}")]
    public async Task<ActionResult<ProductDto>> GetById(int id)
        => Ok(await _service.GetByIdAsync(id));

    /// <summary>Crée un nouveau produit.</summary>
    [HttpPost]
    public async Task<ActionResult<ProductDto>> Create([FromBody] ProductUpsertDto dto)
    {
        var created = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    /// <summary>Modifie un produit existant.</summary>
    [HttpPut("{id:int}")]
    public async Task<ActionResult<ProductDto>> Update(int id, [FromBody] ProductUpsertDto dto)
        => Ok(await _service.UpdateAsync(id, dto));

    /// <summary>Supprime un produit.</summary>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _service.DeleteAsync(id);
        return NoContent();
    }
}
