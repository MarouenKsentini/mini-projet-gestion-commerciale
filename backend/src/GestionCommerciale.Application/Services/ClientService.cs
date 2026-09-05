using GestionCommerciale.Application.DTOs;
using GestionCommerciale.Application.Interfaces;
using GestionCommerciale.Domain.Entities;
using GestionCommerciale.Domain.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace GestionCommerciale.Application.Services;

public class ClientService : IClientService
{
    private readonly IAppDbContext _db;

    public ClientService(IAppDbContext db) => _db = db;

    public async Task<List<ClientDto>> GetAllAsync()
    {
        return await _db.Clients
            .OrderByDescending(c => c.DateCreation)
            .Select(c => ToDto(c))
            .ToListAsync();
    }

    public async Task<ClientDto> GetByIdAsync(int id)
    {
        var client = await _db.Clients.FindAsync(id)
            ?? throw new NotFoundException($"Client {id} introuvable.");
        return ToDto(client);
    }

    public async Task<ClientDto> CreateAsync(ClientUpsertDto dto)
    {
        var emailExists = await _db.Clients.AnyAsync(c => c.Email == dto.Email);
        if (emailExists)
            throw new BusinessException($"Un client avec l'email '{dto.Email}' existe déjà.");

        var client = new Client
        {
            Nom = dto.Nom,
            PrenomOuRaisonSociale = dto.PrenomOuRaisonSociale,
            Email = dto.Email,
            Telephone = dto.Telephone,
            Adresse = dto.Adresse,
            DateCreation = DateTime.UtcNow
        };

        _db.Clients.Add(client);
        await _db.SaveChangesAsync();

        return ToDto(client);
    }

    public async Task<ClientDto> UpdateAsync(int id, ClientUpsertDto dto)
    {
        var client = await _db.Clients.FindAsync(id)
            ?? throw new NotFoundException($"Client {id} introuvable.");

        var emailUsedByOther = await _db.Clients.AnyAsync(c => c.Email == dto.Email && c.Id != id);
        if (emailUsedByOther)
            throw new BusinessException($"Un autre client utilise déjà l'email '{dto.Email}'.");

        client.Nom = dto.Nom;
        client.PrenomOuRaisonSociale = dto.PrenomOuRaisonSociale;
        client.Email = dto.Email;
        client.Telephone = dto.Telephone;
        client.Adresse = dto.Adresse;

        await _db.SaveChangesAsync();

        return ToDto(client);
    }

    public async Task DeleteAsync(int id)
    {
        var client = await _db.Clients.FindAsync(id)
            ?? throw new NotFoundException($"Client {id} introuvable.");

        var hasOrders = await _db.Orders.AnyAsync(o => o.ClientId == id);
        if (hasOrders)
            throw new BusinessException("Impossible de supprimer un client qui a des commandes associées.");

        _db.Clients.Remove(client);
        await _db.SaveChangesAsync();
    }

    private static ClientDto ToDto(Client c) => new(
        c.Id, c.Nom, c.PrenomOuRaisonSociale, c.Email, c.Telephone, c.Adresse, c.DateCreation);
}
