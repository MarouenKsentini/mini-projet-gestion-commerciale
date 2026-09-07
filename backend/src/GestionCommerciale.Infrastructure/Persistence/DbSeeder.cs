using GestionCommerciale.Domain.Entities;
using GestionCommerciale.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace GestionCommerciale.Infrastructure.Persistence;

public static class DbSeeder
{
    public static async Task SeedAsync(AppDbContext db)
    {
        if (await db.Clients.AnyAsync()) return; // already seeded

        var c1 = new Client { Nom = "Ben Ali", PrenomOuRaisonSociale = "Ahmed", Email = "ahmed.benali@test.tn", Telephone = "20001000", Adresse = "Tunis" };
        var c2 = new Client { Nom = "XYZ", PrenomOuRaisonSociale = "Societe XYZ", Email = "contact@xyz.tn", Telephone = "71000000", Adresse = "Sfax" };
        db.Clients.AddRange(c1, c2);

        var p1 = new Product { Reference = "REF-001", Nom = "Clavier AZERTY", Description = "Clavier filaire", PrixUnitaireHT = 85.50m, QuantiteStock = 50 };
        var p2 = new Product { Reference = "REF-002", Nom = "Souris optique", Description = "Souris USB", PrixUnitaireHT = 45.00m, QuantiteStock = 100 };
        var p3 = new Product { Reference = "REF-003", Nom = "Ecran 24 pouces", Description = "Full HD", PrixUnitaireHT = 450.00m, QuantiteStock = 20 };
        var p4 = new Product { Reference = "REF-004", Nom = "Cable HDMI 2m", PrixUnitaireHT = 15.00m, QuantiteStock = 200 };
        db.Products.AddRange(p1, p2, p3, p4);
        await db.SaveChangesAsync();

        var order = new Order
        {
            NumeroCommande = $"CMD-SEED-001-{DateTime.UtcNow:HHmmss}",
            ClientId = c1.Id,
            Statut = OrderStatus.Brouillon,
            TotalHT = 2 * p1.PrixUnitaireHT + 5 * p2.PrixUnitaireHT, // 396
            TotalTTC = Math.Round((2 * p1.PrixUnitaireHT + 5 * p2.PrixUnitaireHT) * 1.19m, 2) // 471.24
        };
        order.Lines.Add(new OrderLine { ProductId = p1.Id, Quantite = 2, PrixUnitaire = p1.PrixUnitaireHT });
        order.Lines.Add(new OrderLine { ProductId = p2.Id, Quantite = 5, PrixUnitaire = p2.PrixUnitaireHT });
        db.Orders.Add(order);
        await db.SaveChangesAsync();
    }
}

public class DbSeederHostedService : IHostedService
{
    private readonly IServiceProvider _sp;
    public DbSeederHostedService(IServiceProvider sp) => _sp = sp;
    public async Task StartAsync(CancellationToken ct)
    {
        using var scope = _sp.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Database.MigrateAsync(ct);
        await DbSeeder.SeedAsync(db);
    }
    public Task StopAsync(CancellationToken ct) => Task.CompletedTask;
}
