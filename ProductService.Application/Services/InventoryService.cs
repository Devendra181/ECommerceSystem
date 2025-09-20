using ProductService.Application.DTOs;
using ProductService.Application.Interfaces;
using ProductService.Domain.Repositories;

namespace ProductService.Application.Services
{
    public class InventoryService : IInventoryService
    {
        private readonly IInventoryRepository _inventoryRepository;

        public InventoryService(IInventoryRepository inventoryRepository)
        {
            _inventoryRepository = inventoryRepository;
        }

        public async Task<bool> IsStockAvailableAsync(Guid productId, int quantity)
        {
            return await _inventoryRepository.IsStockAvailableAsync(productId, quantity);
        }

        public async Task UpdateStockAsync(InventoryUpdateDTO inventoryUpdateDto)
        {
            await _inventoryRepository.UpdateStockAsync(inventoryUpdateDto.ProductId, inventoryUpdateDto.Quantity);
        }

        public async Task IncreaseStockBulkAsync(List<InventoryUpdateDTO> stockUpdates)
        {
            var updates = stockUpdates.Select(s => (s.ProductId, s.Quantity));
            await _inventoryRepository.IncreaseStockBulkAsync(updates);
        }

        public async Task DecreaseStockBulkAsync(List<InventoryUpdateDTO> stockUpdates)
        {
            var updates = stockUpdates.Select(s => (s.ProductId, s.Quantity));
            await _inventoryRepository.DecreaseStockBulkAsync(updates);
        }

        public async Task<List<ProductStockInfoResponseDTO>> VerifyStockForProductsAsync(List<ProductStockInfoRequestDTO> requestedItems)
        {
            var productIds = requestedItems.Select(x => x.ProductId).Distinct();

            var products = await _inventoryRepository.GetProductsByIdsAsync(productIds);

            var results = new List<ProductStockInfoResponseDTO>();

            foreach (var item in requestedItems)
            {
                var product = products.FirstOrDefault(p => p.Id == item.ProductId);

                if (product == null)
                {
                    results.Add(new ProductStockInfoResponseDTO
                    {
                        ProductId = item.ProductId,
                        IsValidProduct = false,
                        IsQuantityAvailable = false,
                        AvailableQuantity = 0
                    });
                }
                else
                {
                    bool isAvailable = product.StockQuantity >= item.Quantity;
                    results.Add(new ProductStockInfoResponseDTO
                    {
                        ProductId = item.ProductId,
                        IsValidProduct = true,
                        IsQuantityAvailable = isAvailable,
                        AvailableQuantity = product.StockQuantity
                    });
                }
            }

            return results;
        }
    }
}
