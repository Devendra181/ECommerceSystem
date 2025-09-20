using AutoMapper;
using OrderService.Application.DTOs.Shipments;
using OrderService.Application.Interfaces;
using OrderService.Domain.Entities;
using OrderService.Domain.Enums;
using OrderService.Domain.Repositories;

namespace OrderService.Application.Services
{
    public class ShipmentService : IShipmentService
    {
        private readonly IShipmentRepository _shipmentRepository;
        private readonly IMapper _mapper;

        public ShipmentService(IShipmentRepository shipmentRepository, IMapper mapper)
        {
            _shipmentRepository = shipmentRepository ?? throw new ArgumentNullException(nameof(shipmentRepository));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<ShipmentResponseDTO> CreateShipmentAsync(CreateShipmentRequestDTO request)
        {
            try
            {
                if (request == null)
                    throw new ArgumentNullException(nameof(request));

                // Basic validation example
                if (request.OrderId == Guid.Empty)
                    throw new ArgumentException("OrderId must be provided.", nameof(request.OrderId));

                if (string.IsNullOrWhiteSpace(request.CarrierName))
                    throw new ArgumentException("CarrierName is required.", nameof(request.CarrierName));

                var shipmentEntity = _mapper.Map<Shipment>(request);

                var createdShipment = await _shipmentRepository.AddAsync(shipmentEntity, request.Location);

                if (createdShipment == null)
                    throw new InvalidOperationException("Failed to create shipment.");

                return _mapper.Map<ShipmentResponseDTO>(createdShipment);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }
        }

        public async Task<ShipmentResponseDTO?> GetShipmentByIdAsync(Guid shipmentId)
        {
            if (shipmentId == Guid.Empty)
                throw new ArgumentException("Invalid shipmentId.", nameof(shipmentId));

            var shipment = await _shipmentRepository.GetByIdAsync(shipmentId);
            if (shipment == null) return null;

            return _mapper.Map<ShipmentResponseDTO>(shipment);
        }

        public async Task<List<ShipmentResponseDTO>> GetShipmentsByOrderIdAsync(Guid orderId)
        {
            if (orderId == Guid.Empty)
                throw new ArgumentException("Invalid orderId.", nameof(orderId));

            var shipments = await _shipmentRepository.GetByOrderIdAsync(orderId);
            return _mapper.Map<List<ShipmentResponseDTO>>(shipments);
        }

        public async Task UpdateShipmentStatusAsync(ShipmentStatusUpdateRequestDTO request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            if (request.ShipmentId == Guid.Empty)
                throw new ArgumentException("ShipmentId is required.", nameof(request.ShipmentId));

            if (!Enum.IsDefined(typeof(ShipmentStatusEnum), request.Status))
                throw new ArgumentException("Invalid shipment status.", nameof(request.Status));

            var shipment = await _shipmentRepository.GetByIdAsync(request.ShipmentId);
            if (shipment == null)
                throw new KeyNotFoundException("Shipment not found.");

            shipment.ShipmentStatusId = (int)request.Status;

            await _shipmentRepository.UpdateAsync(shipment, request.ChangedBy ?? "System", request.Remarks, request.Location);
        }

        public async Task<ShipmentTrackingResponseDTO> TrackShipmentAsync(ShipmentTrackingRequestDTO request)
        {
            try
            {
                if (request == null)
                    throw new ArgumentNullException(nameof(request));

                var shipment = await _shipmentRepository.GetByTrackingNumberAsync(request.TrackingNumber);
                if (shipment == null)
                    throw new KeyNotFoundException("Shipment not found.");

                var shipmentDto = _mapper.Map<ShipmentTrackingResponseDTO>(shipment);

                shipmentDto.StatusHistories = _mapper.Map<List<ShipmentStatusHistoryDTO>>(shipment.ShipmentStatusHistories.OrderBy(h => h.ChangedAt));

                return shipmentDto;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }
           
        }
    }
}
