using GestionCommerciale.Application.DTOs;

namespace GestionCommerciale.Application.Interfaces;

public interface IOrderService
{
    Task<List<OrderDto>> GetAllAsync();
    Task<OrderDto> GetByIdAsync(int id);
    Task<OrderDto> CreateAsync(OrderUpsertDto dto);
    Task<OrderDto> UpdateAsync(int id, OrderUpsertDto dto);
    Task DeleteAsync(int id);
    Task<OrderDto> ValidateAsync(int id);
}
