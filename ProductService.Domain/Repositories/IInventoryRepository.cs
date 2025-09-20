using ProductService.Domain.Entities;

namespace ProductService.Domain.Repositories
{
    public interface IInventoryRepository
    {
        Task<bool> IsStockAvailableAsync(Guid productId, int quantity);
        Task UpdateStockAsync(Guid productId, int quantityChange);
        Task IncreaseStockBulkAsync(IEnumerable<(Guid productId, int quantity)> stockUpdates);
        Task DecreaseStockBulkAsync(IEnumerable<(Guid productId, int quantity)> stockUpdates);
        Task<List<Product>> GetProductsByIdsAsync(IEnumerable<Guid> productIds);
    }
}
