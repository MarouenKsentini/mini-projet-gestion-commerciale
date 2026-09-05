using System.ComponentModel.DataAnnotations;

namespace GestionCommerciale.Application.DTOs;

public record ProductDto(
    int Id,
    string Reference,
    string Nom,
    string? Description,
    decimal PrixUnitaireHT,
    int QuantiteStock,
    DateTime DateCreation);

public class ProductUpsertDto
{
    [Required(ErrorMessage = "La référence est obligatoire.")]
    [MaxLength(50)]
    public string Reference { get; set; } = default!;

    [Required(ErrorMessage = "Le nom du produit est obligatoire.")]
    [MaxLength(150)]
    public string Nom { get; set; } = default!;

    [MaxLength(500)]
    public string? Description { get; set; }

    [Range(0.01, double.MaxValue, ErrorMessage = "Le prix unitaire doit être supérieur à zéro.")]
    public decimal PrixUnitaireHT { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "Le stock ne peut pas être négatif.")]
    public int QuantiteStock { get; set; }
}
