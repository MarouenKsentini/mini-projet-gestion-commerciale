using GestionCommerciale.Application.DTOs;
using GestionCommerciale.Application.Interfaces;
using GestionCommerciale.Domain.Entities;
using GestionCommerciale.Domain.Enums;
using GestionCommerciale.Domain.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace GestionCommerciale.Application.Services;

public class OrderService : IOrderService
{
    private const decimal TauxTVA = 0.19m;

    private readonly IAppDbContext _db;

    public OrderService(IAppDbContext db) => _db = db;

    public async Task<List<OrderDto>> GetAllAsync()
    {
        var orders = await _db.Orders
            .Include(o => o.Client)
            .Include(o => o.Lines).ThenInclude(l => l.Product)
            .OrderByDescending(o => o.DateCommande)
            .ToListAsync();

        return orders.Select(ToDto).ToList();
    }

    public async Task<OrderDto> GetByIdAsync(int id)
    {
        var order = await GetOrderWithDetailsAsync(id);
        return ToDto(order);
    }

    public async Task<OrderDto> CreateAsync(OrderUpsertDto dto)
    {
        var client = await _db.Clients.FindAsync(dto.ClientId)
            ?? throw new BusinessException("Client introuvable : impossible de créer une commande sans client.");

        if (dto.Lines is null || dto.Lines.Count == 0)
            throw new BusinessException("Une commande doit contenir au moins une ligne.");

        var order = new Order
        {
            NumeroCommande = GenerateNumeroCommande(),
            ClientId = client.Id,
            DateCommande = DateTime.UtcNow,
            Statut = OrderStatus.Brouillon
        };

        await BuildLinesAsync(order, dto.Lines);
        RecalculateTotals(order);

        _db.Orders.Add(order);
        await _db.SaveChangesAsync();

        var created = await GetOrderWithDetailsAsync(order.Id);
        return ToDto(created);
    }

    public async Task<OrderDto> UpdateAsync(int id, OrderUpsertDto dto)
    {
        var order = await GetOrderWithDetailsAsync(id);

        if (order.Statut != OrderStatus.Brouillon)
            throw new BusinessException("Seule une commande en brouillon peut être modifiée.");

        var client = await _db.Clients.FindAsync(dto.ClientId)
            ?? throw new BusinessException("Client introuvable : impossible d'affecter la commande à ce client.");

        if (dto.Lines is null || dto.Lines.Count == 0)
            throw new BusinessException("Une commande doit contenir au moins une ligne.");

        order.ClientId = client.Id;

        // On repart d'une liste de lignes propre pour éviter les incohérences de mise à jour partielle.
        order.Lines.Clear();
        await BuildLinesAsync(order, dto.Lines);
        RecalculateTotals(order);

        await _db.SaveChangesAsync();

        var updated = await GetOrderWithDetailsAsync(order.Id);
        return ToDto(updated);
    }

    public async Task DeleteAsync(int id)
    {
        var order = await _db.Orders.FindAsync(id)
            ?? throw new NotFoundException($"Commande {id} introuvable.");

        if (order.Statut == OrderStatus.Validee)
            throw new BusinessException("Impossible de supprimer une commande déjà validée.");

        _db.Orders.Remove(order);
        await _db.SaveChangesAsync();
    }

    public async Task<OrderDto> ValidateAsync(int id)
    {
        var order = await GetOrderWithDetailsAsync(id);

        if (order.Statut != OrderStatus.Brouillon)
            throw new BusinessException("Seule une commande en brouillon peut être validée.");

        // On revérifie le stock au moment de la validation : il a pu changer depuis la création.
        foreach (var line in order.Lines)
        {
            if (line.Quantite > line.Product.QuantiteStock)
                throw new BusinessException(
                    $"Stock insuffisant pour '{line.Product.Nom}' (disponible : {line.Product.QuantiteStock}, demandé : {line.Quantite}).");
        }

        foreach (var line in order.Lines)
            line.Product.QuantiteStock -= line.Quantite;

        order.Statut = OrderStatus.Validee;

        await _db.SaveChangesAsync();

        return ToDto(order);
    }

    public async Task<OrderDto> CancelAsync(int id)
    {
        var order = await GetOrderWithDetailsAsync(id);

        if (order.Statut != OrderStatus.Brouillon)
            throw new BusinessException("Seule une commande en brouillon peut être annulée.");

        order.Statut = OrderStatus.Annulee;

        await _db.SaveChangesAsync();

        return ToDto(order);
    }

    // ---- Helpers privés ----

    private async Task BuildLinesAsync(Order order, List<OrderLineUpsertDto> linesDto)
    {
        foreach (var lineDto in linesDto)
        {
            if (lineDto.Quantite <= 0)
                throw new BusinessException("Il ne doit pas être possible de créer une ligne avec une quantité inférieure ou égale à zéro.");

            var product = await _db.Products.FindAsync(lineDto.ProductId)
                ?? throw new BusinessException($"Produit {lineDto.ProductId} introuvable.");

            if (lineDto.Quantite > product.QuantiteStock)
                throw new BusinessException(
                    $"Impossible de commander {lineDto.Quantite} x '{product.Nom}' : stock disponible = {product.QuantiteStock}.");

            order.Lines.Add(new OrderLine
            {
                ProductId = product.Id,
                Product = product,
                Quantite = lineDto.Quantite,
                PrixUnitaire = product.PrixUnitaireHT
            });
        }
    }

    private static void RecalculateTotals(Order order)
    {
        order.TotalHT = order.Lines.Sum(l => l.Quantite * l.PrixUnitaire);
        order.TotalTTC = Math.Round(order.TotalHT * (1 + TauxTVA), 2);
    }

    private async Task<Order> GetOrderWithDetailsAsync(int id)
    {
        return await _db.Orders
            .Include(o => o.Client)
            .Include(o => o.Lines).ThenInclude(l => l.Product)
            .FirstOrDefaultAsync(o => o.Id == id)
            ?? throw new NotFoundException($"Commande {id} introuvable.");
    }

    private static string GenerateNumeroCommande()
        => $"CMD-{DateTime.UtcNow:yyyyMMddHHmmssfff}";

    private static OrderDto ToDto(Order o) => new(
        o.Id,
        o.NumeroCommande,
        o.ClientId,
        $"{o.Client.Nom} {o.Client.PrenomOuRaisonSociale}",
        o.DateCommande,
        o.Statut.ToString(),
        o.TotalHT,
        o.TotalTTC,
        o.Lines.Select(l => new OrderLineDto(
            l.Id, l.ProductId, l.Product.Nom, l.Quantite, l.PrixUnitaire, l.TotalLigne)).ToList());
}
