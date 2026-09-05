using GestionCommerciale.Domain.Enums;

namespace GestionCommerciale.Domain.Entities;

public class Order
{
    public int Id { get; set; }
    public string NumeroCommande { get; set; } = default!;

    public int ClientId { get; set; }
    public Client Client { get; set; } = default!;

    public DateTime DateCommande { get; set; } = DateTime.UtcNow;
    public OrderStatus Statut { get; set; } = OrderStatus.Brouillon;

    public decimal TotalHT { get; set; }
    public decimal TotalTTC { get; set; }

    public ICollection<OrderLine> Lines { get; set; } = new List<OrderLine>();
}
