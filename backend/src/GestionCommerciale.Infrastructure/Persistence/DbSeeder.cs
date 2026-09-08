using GestionCommerciale.Domain.Entities;
using GestionCommerciale.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace GestionCommerciale.Infrastructure.Persistence;

public static class DbSeeder
{
    public static async Task SeedAsync(AppDbContext db)
    {
        if (await db.Clients.AnyAsync()) return; // already seeded

        var c1 = new Client { Nom = "Ben Ali", PrenomOuRaisonSociale = "Ahmed", Email = "ahmed.benali@test.tn", Telephone = "20001000", Adresse = "Tunis" };
        var c2 = new Client { Nom = "XYZ", PrenomOuRaisonSociale = "Societe XYZ", Email = "contact@xyz.tn", Telephone = "71000000", Adresse = "Sfax" };
        var c3 = new Client { Nom = "Trabelsi", PrenomOuRaisonSociale = "Sami", Email = "sami.trabelsi@test.tn", Telephone = "22001122", Adresse = "Sousse" };
        var c4 = new Client { Nom = "ABC", PrenomOuRaisonSociale = "Ste ABC", Email = "contact@abc.tn", Telephone = "70112233", Adresse = "Monastir" };
        var c5 = new Client { Nom = "Mansour", PrenomOuRaisonSociale = "Leila", Email = "leila.mansour@test.tn", Telephone = "23112233", Adresse = "Hammamet" };
        var c6 = new Client { Nom = "DEF", PrenomOuRaisonSociale = "Ste DEF", Email = "contact@def.tn", Telephone = "70223344", Adresse = "Gabes" };
        var c7 = new Client { Nom = "Gharbi", PrenomOuRaisonSociale = "Mohamed", Email = "mohamed.gharbi@test.tn", Telephone = "24001133", Adresse = "Bizerte" };
        var c8 = new Client { Nom = "GHI", PrenomOuRaisonSociale = "Ste GHI", Email = "contact@ghi.tn", Telephone = "70334455", Adresse = "Kairouan" };
        db.Clients.AddRange(c1, c2, c3, c4, c5, c6, c7, c8);

        var p1 = new Product { Reference = "REF-001", Nom = "Clavier AZERTY", Description = "Clavier filaire", PrixUnitaireHT = 85.50m, QuantiteStock = 50 };
        var p2 = new Product { Reference = "REF-002", Nom = "Souris optique", Description = "Souris USB", PrixUnitaireHT = 45.00m, QuantiteStock = 100 };
        var p3 = new Product { Reference = "REF-003", Nom = "Ecran 24 pouces", Description = "Full HD", PrixUnitaireHT = 450.00m, QuantiteStock = 20 };
        var p4 = new Product { Reference = "REF-004", Nom = "Cable HDMI 2m", PrixUnitaireHT = 15.00m, QuantiteStock = 200 };
        var p5 = new Product { Reference = "REF-005", Nom = "Imprimante laser", Description = "Monochrome A4", PrixUnitaireHT = 620.00m, QuantiteStock = 12 };
        var p6 = new Product { Reference = "REF-006", Nom = "Webcam HD", Description = "1080p USB", PrixUnitaireHT = 95.00m, QuantiteStock = 30 };
        var p7 = new Product { Reference = "REF-007", Nom = "Disque SSD 512Go", Description = "NVMe M.2", PrixUnitaireHT = 180.00m, QuantiteStock = 40 };
        var p8 = new Product { Reference = "REF-008", Nom = "Casque audio", Description = "Bluetooth", PrixUnitaireHT = 120.00m, QuantiteStock = 25 };
        var p9 = new Product { Reference = "REF-009", Nom = "Tapis souris", Description = "XXL", PrixUnitaireHT = 25.00m, QuantiteStock = 60 };
        var p10 = new Product { Reference = "REF-010", Nom = "Clavier mecanique", Description = "RGB", PrixUnitaireHT = 210.00m, QuantiteStock = 18 };
        var p11 = new Product { Reference = "REF-011", Nom = "Hub USB-C", Description = "7 ports", PrixUnitaireHT = 75.00m, QuantiteStock = 35 };
        var p12 = new Product { Reference = "REF-012", Nom = "Ecran 27 pouces", Description = "QHD", PrixUnitaireHT = 690.00m, QuantiteStock = 8 };
        db.Products.AddRange(p1, p2, p3, p4, p5, p6, p7, p8, p9, p10, p11, p12);
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
    private readonly ILogger<DbSeederHostedService> _logger;
    public DbSeederHostedService(IServiceProvider sp, ILogger<DbSeederHostedService> logger) { _sp = sp; _logger = logger; }
    public async Task StartAsync(CancellationToken ct)
    {
        try
        {
            using var scope = _sp.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            // SQLite in sandbox has no SQL Server migrations -> use EnsureCreated
            if (db.Database.IsSqlite())
                await db.Database.EnsureCreatedAsync(ct);
            else
                await db.Database.MigrateAsync(ct);
            await DbSeeder.SeedAsync(db);
            _logger.LogInformation("Database migrated and seeded.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Database migration/seed failed for Server DESKTOP-7VIA6NI\\MAROUEN. Check SQL Server Browser service, instance name, and that Integrated Security login has dbcreator. App will still start — fix appsettings.Development.json and restart, or run backend/database/seed.sql manually. Error 26 = instance not found/spelling or Browser stopped.");
            // Don't throw — let API start so Swagger is reachable and error is visible
        }
    }
    public Task StopAsync(CancellationToken ct) => Task.CompletedTask;
}
