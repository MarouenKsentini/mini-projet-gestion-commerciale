using GestionCommerciale.Application.DTOs;

namespace GestionCommerciale.Application.Interfaces;

public interface IClientService
{
    Task<List<ClientDto>> GetAllAsync();
    Task<ClientDto> GetByIdAsync(int id);
    Task<ClientDto> CreateAsync(ClientUpsertDto dto);
    Task<ClientDto> UpdateAsync(int id, ClientUpsertDto dto);
    Task DeleteAsync(int id);
}
