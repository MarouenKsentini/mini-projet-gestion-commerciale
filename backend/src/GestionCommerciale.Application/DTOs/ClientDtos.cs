using System.ComponentModel.DataAnnotations;

namespace GestionCommerciale.Application.DTOs;

public record ClientDto(
    int Id,
    string Nom,
    string PrenomOuRaisonSociale,
    string Email,
    string? Telephone,
    string? Adresse,
    DateTime DateCreation);

public class ClientUpsertDto
{
    [Required(ErrorMessage = "Le nom est obligatoire.")]
    [MaxLength(100)]
    public string Nom { get; set; } = default!;

    [Required(ErrorMessage = "Le prénom ou la raison sociale est obligatoire.")]
    [MaxLength(150)]
    public string PrenomOuRaisonSociale { get; set; } = default!;

    [Required(ErrorMessage = "L'email est obligatoire.")]
    [EmailAddress(ErrorMessage = "Format d'email invalide.")]
    public string Email { get; set; } = default!;

    [MaxLength(30)]
    public string? Telephone { get; set; }

    [MaxLength(300)]
    public string? Adresse { get; set; }
}
