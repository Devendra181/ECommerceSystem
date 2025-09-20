using OrderService.Domain.Entities;
using OrderService.Domain.Enums;

namespace OrderService.Domain.Repositories
{
    public interface IMasterDataRepository
    {
        Task<List<OrderStatus>> GetOrderStatusesAsync();
        Task<OrderStatus?> GetOrderStatusByIdAsync(int id);

        Task<List<CancellationStatus>> GetCancellationStatusesAsync();
        Task<CancellationStatus?> GetCancellationStatusByIdAsync(int id);

        Task<List<ReturnStatus>> GetReturnStatusesAsync();
        Task<ReturnStatus?> GetReturnStatusByIdAsync(int id);

        Task<List<RefundStatus>> GetRefundStatusesAsync();
        Task<RefundStatus?> GetRefundStatusByIdAsync(int id);

        Task<List<ShipmentStatus>> GetShipmentStatusesAsync();
        Task<ShipmentStatus?> GetShipmentStatusByIdAsync(int id);

        Task<List<string>> GetReasonTypesAsync();
        Task<List<ReasonMaster>> GetReasonsByTypeAsync(ReasonTypeEnum reasonType);

        Task<List<CancellationPolicy>> GetCancellationPoliciesAsync();
        Task<CancellationPolicy?> GetCancellationPolicyByIdAsync(int id);
        Task<CancellationPolicy?> GetActiveCancellationPolicyAsync();

        Task<List<ReturnPolicy>> GetReturnPoliciesAsync();
        Task<ReturnPolicy?> GetReturnPolicyByIdAsync(int id);
        Task<ReturnPolicy?> GetActiveReturnPolicyAsync();

        Task<List<Discount>> GetDiscountsAsync();
        Task<Discount?> GetDiscountByIdAsync(int id);
        Task<List<Discount>> GetActiveDiscountsAsync(DateTime asOfDate);

        Task<List<Tax>> GetTaxesAsync();
        Task<Tax?> GetTaxByIdAsync(int id);
        Task<List<Tax>> GetActiveTaxesAsync(DateTime asOfDate);
    }
}

