using OrderService.Domain.Entities;

namespace OrderService.Domain.Repositories
{
    public interface IShipmentRepository
    {
        Task<Shipment> AddAsync(Shipment shipment, string Location);
        Task<List<Shipment>> GetByOrderIdAsync(Guid orderId);
        Task<Shipment?> GetByIdAsync(Guid shipmentId);
        Task<Shipment?> UpdateAsync(Shipment shipment, string? changedBy = null, string? remarks = null, string? location = null);
        Task<Shipment?> GetByTrackingNumberAsync(string TrackingNumber);
    }
}

