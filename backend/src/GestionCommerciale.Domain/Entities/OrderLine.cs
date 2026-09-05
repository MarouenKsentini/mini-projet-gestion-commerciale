namespace GestionCommerciale.Domain.Entities;

public class OrderLine
{
    public int Id { get; set; }

    public int OrderId { get; set; }
    public Order Order { get; set; } = default!;

    public int ProductId { get; set; }
    public Product Product { get; set; } = default!;

    public int Quantite { get; set; }
    public decimal PrixUnitaire { get; set; }

    public decimal TotalLigne => Quantite * PrixUnitaire;
}
