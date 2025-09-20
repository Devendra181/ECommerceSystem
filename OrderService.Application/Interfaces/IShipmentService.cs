using OrderService.Application.DTOs.Shipments;

namespace OrderService.Application.Interfaces
{
    public interface IShipmentService
    {
        Task<ShipmentResponseDTO> CreateShipmentAsync(CreateShipmentRequestDTO request);
        Task<ShipmentResponseDTO?> GetShipmentByIdAsync(Guid shipmentId);
        Task<List<ShipmentResponseDTO>> GetShipmentsByOrderIdAsync(Guid orderId);
        Task UpdateShipmentStatusAsync(ShipmentStatusUpdateRequestDTO request);
        Task<ShipmentTrackingResponseDTO> TrackShipmentAsync(ShipmentTrackingRequestDTO request);
    }
}