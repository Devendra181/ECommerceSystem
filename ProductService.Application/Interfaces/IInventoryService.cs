using ProductService.Application.DTOs;

namespace ProductService.Application.Interfaces
{
    public interface IInventoryService
    {
        Task<bool> IsStockAvailableAsync(Guid productId, int quantity);
        Task UpdateStockAsync(InventoryUpdateDTO inventoryUpdateDto);
        Task IncreaseStockBulkAsync(List<InventoryUpdateDTO> stockUpdates);
        Task DecreaseStockBulkAsync(List<InventoryUpdateDTO> stockUpdates);
        Task<List<ProductStockInfoResponseDTO>> VerifyStockForProductsAsync(List<ProductStockInfoRequestDTO> requestedItems);
    }
}
