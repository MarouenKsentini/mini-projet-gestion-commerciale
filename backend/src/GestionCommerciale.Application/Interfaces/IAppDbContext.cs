using GestionCommerciale.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace GestionCommerciale.Application.Interfaces;

/// <summary>
/// Abstraction du DbContext pour garder l'Application layer indépendante d'EF Core
/// concret (implémentée par AppDbContext dans Infrastructure).
/// </summary>
public interface IAppDbContext
{
    DbSet<Client> Clients { get; }
    DbSet<Product> Products { get; }
    DbSet<Order> Orders { get; }
    DbSet<OrderLine> OrderLines { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
