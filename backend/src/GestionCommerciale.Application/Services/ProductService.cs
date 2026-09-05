using GestionCommerciale.Application.DTOs;
using GestionCommerciale.Application.Interfaces;
using GestionCommerciale.Domain.Entities;
using GestionCommerciale.Domain.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace GestionCommerciale.Application.Services;

public class ProductService : IProductService
{
    private readonly IAppDbContext _db;

    public ProductService(IAppDbContext db) => _db = db;

    public async Task<List<ProductDto>> GetAllAsync()
    {
        return await _db.Products
            .OrderByDescending(p => p.DateCreation)
            .Select(p => ToDto(p))
            .ToListAsync();
    }

    public async Task<ProductDto> GetByIdAsync(int id)
    {
        var product = await _db.Products.FindAsync(id)
            ?? throw new NotFoundException($"Produit {id} introuvable.");
        return ToDto(product);
    }

    public async Task<ProductDto> CreateAsync(ProductUpsertDto dto)
    {
        var refExists = await _db.Products.AnyAsync(p => p.Reference == dto.Reference);
        if (refExists)
            throw new BusinessException($"La référence '{dto.Reference}' est déjà utilisée.");

        var product = new Product
        {
            Reference = dto.Reference,
            Nom = dto.Nom,
            Description = dto.Description,
            PrixUnitaireHT = dto.PrixUnitaireHT,
            QuantiteStock = dto.QuantiteStock,
            DateCreation = DateTime.UtcNow
        };

        _db.Products.Add(product);
        await _db.SaveChangesAsync();

        return ToDto(product);
    }

    public async Task<ProductDto> UpdateAsync(int id, ProductUpsertDto dto)
    {
        var product = await _db.Products.FindAsync(id)
            ?? throw new NotFoundException($"Produit {id} introuvable.");

        var refUsedByOther = await _db.Products.AnyAsync(p => p.Reference == dto.Reference && p.Id != id);
        if (refUsedByOther)
            throw new BusinessException($"La référence '{dto.Reference}' est déjà utilisée par un autre produit.");

        product.Reference = dto.Reference;
        product.Nom = dto.Nom;
        product.Description = dto.Description;
        product.PrixUnitaireHT = dto.PrixUnitaireHT;
        product.QuantiteStock = dto.QuantiteStock;

        await _db.SaveChangesAsync();

        return ToDto(product);
    }

    public async Task DeleteAsync(int id)
    {
        var product = await _db.Products.FindAsync(id)
            ?? throw new NotFoundException($"Produit {id} introuvable.");

        var usedInOrders = await _db.OrderLines.AnyAsync(l => l.ProductId == id);
        if (usedInOrders)
            throw new BusinessException("Impossible de supprimer un produit déjà utilisé dans une commande.");

        _db.Products.Remove(product);
        await _db.SaveChangesAsync();
    }

    private static ProductDto ToDto(Product p) => new(
        p.Id, p.Reference, p.Nom, p.Description, p.PrixUnitaireHT, p.QuantiteStock, p.DateCreation);
}
