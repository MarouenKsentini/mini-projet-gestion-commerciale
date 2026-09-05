namespace GestionCommerciale.Domain.Entities;

public class Client
{
    public int Id { get; set; }
    public string Nom { get; set; } = default!;
    public string PrenomOuRaisonSociale { get; set; } = default!;
    public string Email { get; set; } = default!;
    public string? Telephone { get; set; }
    public string? Adresse { get; set; }
    public DateTime DateCreation { get; set; } = DateTime.UtcNow;

    public ICollection<Order> Orders { get; set; } = new List<Order>();
}
