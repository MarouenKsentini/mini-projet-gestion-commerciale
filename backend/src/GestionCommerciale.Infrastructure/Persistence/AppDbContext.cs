using GestionCommerciale.Application.Interfaces;
using GestionCommerciale.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace GestionCommerciale.Infrastructure.Persistence;

public class AppDbContext : DbContext, IAppDbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Client> Clients => Set<Client>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderLine> OrderLines => Set<OrderLine>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Client>(entity =>
        {
            entity.Property(c => c.Nom).HasMaxLength(100).IsRequired();
            entity.Property(c => c.PrenomOuRaisonSociale).HasMaxLength(150).IsRequired();
            entity.Property(c => c.Email).HasMaxLength(150).IsRequired();
            entity.Property(c => c.Telephone).HasMaxLength(30);
            entity.Property(c => c.Adresse).HasMaxLength(300);
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.Property(p => p.Reference).HasMaxLength(50).IsRequired();
            entity.HasIndex(p => p.Reference).IsUnique();
            entity.Property(p => p.Nom).HasMaxLength(150).IsRequired();
            entity.Property(p => p.Description).HasMaxLength(500);
            entity.Property(p => p.PrixUnitaireHT).HasColumnType("decimal(18,2)");
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.Property(o => o.NumeroCommande).HasMaxLength(50).IsRequired();
            entity.HasIndex(o => o.NumeroCommande).IsUnique();
            entity.Property(o => o.Statut).HasConversion<string>().HasMaxLength(20);
            entity.Property(o => o.TotalHT).HasColumnType("decimal(18,2)");
            entity.Property(o => o.TotalTTC).HasColumnType("decimal(18,2)");

            entity.HasOne(o => o.Client)
                  .WithMany(c => c.Orders)
                  .HasForeignKey(o => o.ClientId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<OrderLine>(entity =>
        {
            entity.Property(l => l.PrixUnitaire).HasColumnType("decimal(18,2)");
            entity.Ignore(l => l.TotalLigne); // calculé en mémoire, pas stocké

            entity.HasOne(l => l.Order)
                  .WithMany(o => o.Lines)
                  .HasForeignKey(l => l.OrderId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(l => l.Product)
                  .WithMany()
                  .HasForeignKey(l => l.ProductId)
                  .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
