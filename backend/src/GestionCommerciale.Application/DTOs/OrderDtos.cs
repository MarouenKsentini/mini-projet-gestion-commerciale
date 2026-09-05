using System.ComponentModel.DataAnnotations;

namespace GestionCommerciale.Application.DTOs;

public record OrderLineDto(
    int Id,
    int ProductId,
    string ProductNom,
    int Quantite,
    decimal PrixUnitaire,
    decimal TotalLigne);

public record OrderDto(
    int Id,
    string NumeroCommande,
    int ClientId,
    string ClientNom,
    DateTime DateCommande,
    string Statut,
    decimal TotalHT,
    decimal TotalTTC,
    List<OrderLineDto> Lines);

public class OrderLineUpsertDto
{
    [Required]
    public int ProductId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "La quantité doit être supérieure à zéro.")]
    public int Quantite { get; set; }
}

public class OrderUpsertDto
{
    [Required(ErrorMessage = "Le client est obligatoire.")]
    public int ClientId { get; set; }

    [MinLength(1, ErrorMessage = "La commande doit contenir au moins une ligne.")]
    public List<OrderLineUpsertDto> Lines { get; set; } = new();
}
