using GestionCommerciale.Application.DTOs;

namespace GestionCommerciale.Application.Interfaces;

public interface IProductService
{
    Task<List<ProductDto>> GetAllAsync();
    Task<ProductDto> GetByIdAsync(int id);
    Task<ProductDto> CreateAsync(ProductUpsertDto dto);
    Task<ProductDto> UpdateAsync(int id, ProductUpsertDto dto);
    Task DeleteAsync(int id);
}
