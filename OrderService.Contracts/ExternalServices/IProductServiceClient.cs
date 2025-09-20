using OrderService.Contracts.DTOs;
namespace OrderService.Contracts.ExternalServices
{
    public interface IProductServiceClient
    {
        Task<ProductDTO?> GetProductByIdAsync(Guid productId);
        Task<List<ProductDTO>?> GetProductsByIdsAsync(List<Guid> productIds, string accessToken);
        Task<List<ProductStockVerificationResponseDTO>?> CheckProductsAvailabilityAsync(List<ProductStockVerificationRequestDTO> requestedItems, string accessToken);
        Task<bool> IncreaseStockBulkAsync(IEnumerable<UpdateStockRequestDTO> stockUpdates, string accessToken);
        Task<bool> DecreaseStockBulkAsync(IEnumerable<UpdateStockRequestDTO> stockUpdates, string accessToken);
    }
}
