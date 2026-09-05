namespace GestionCommerciale.Domain.Entities;

public class Product
{
    public int Id { get; set; }
    public string Reference { get; set; } = default!;
    public string Nom { get; set; } = default!;
    public string? Description { get; set; }
    public decimal PrixUnitaireHT { get; set; }
    public int QuantiteStock { get; set; }
    public DateTime DateCreation { get; set; } = DateTime.UtcNow;
}
