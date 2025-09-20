using Microsoft.EntityFrameworkCore;
using OrderService.Domain.Entities;
using OrderService.Domain.Enums;
using OrderService.Domain.Repositories;
using OrderService.Infrastructure.Persistence;

namespace OrderService.Infrastructure.Repositories
{
    public class ShipmentRepository : IShipmentRepository
    {
        private readonly OrderDbContext _dbContext;

        private static readonly Dictionary<ShipmentStatusEnum, List<ShipmentStatusEnum>> AllowedTransitions = new()
        {
            { ShipmentStatusEnum.Pending, new List<ShipmentStatusEnum> { ShipmentStatusEnum.Shipped, ShipmentStatusEnum.Cancelled } },
            { ShipmentStatusEnum.Shipped, new List<ShipmentStatusEnum> { ShipmentStatusEnum.InTransit, ShipmentStatusEnum.Cancelled } },
            { ShipmentStatusEnum.InTransit, new List<ShipmentStatusEnum> { ShipmentStatusEnum.OutForDelivery, ShipmentStatusEnum.Cancelled } },
            { ShipmentStatusEnum.OutForDelivery, new List<ShipmentStatusEnum> { ShipmentStatusEnum.Delivered, ShipmentStatusEnum.Returned } },
            { ShipmentStatusEnum.Delivered, new List<ShipmentStatusEnum>() },
            { ShipmentStatusEnum.Cancelled, new List<ShipmentStatusEnum>() },
            { ShipmentStatusEnum.Returned, new List<ShipmentStatusEnum>() }
        };

        public ShipmentRepository(OrderDbContext dbContext)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        public async Task<Shipment> AddAsync(Shipment shipment, string Location)
        {
            if (shipment == null)
                throw new ArgumentNullException(nameof(shipment));

            shipment.IsDeleted = false;
            shipment.ShipmentStatusId = (int)ShipmentStatusEnum.Shipped;
            shipment.CreatedAt = DateTime.UtcNow;
            shipment.UpdatedAt = DateTime.UtcNow;

            await _dbContext.Shipments.AddAsync(shipment);

            // Set ShipmentId on all shipment items
            foreach (var item in shipment.ShipmentItems)
            {
                item.ShipmentId = shipment.Id;
            }

            await _dbContext.Shipments.AddAsync(shipment);

            // Add initial status history record
            var history = new ShipmentStatusHistory
            {
                Id = Guid.NewGuid(),
                ShipmentId = shipment.Id,
                OldStatusId = shipment.ShipmentStatusId,
                NewStatusId = shipment.ShipmentStatusId,
                ChangedBy = null,
                ChangedAt = DateTime.UtcNow,
                Remarks = "Initial status",
                Location = Location
            };
            await _dbContext.ShipmentStatusHistories.AddAsync(history);

            await _dbContext.SaveChangesAsync();

            return await GetByIdAsync(shipment.Id) ?? new Shipment();
        }

        public async Task<List<Shipment>> GetByOrderIdAsync(Guid orderId)
        {
            return await _dbContext.Shipments
                .AsNoTracking()
                .Where(s => s.OrderId == orderId && !s.IsDeleted)
                .Include(s => s.ShipmentStatus)
                .Include(s => s.ShipmentItems)
                .OrderByDescending(s => s.EstimatedDeliveryDate)
                .ToListAsync();
        }

        public async Task<Shipment?> GetByIdAsync(Guid shipmentId)
        {
            if (shipmentId == Guid.Empty)
                throw new ArgumentException("Invalid shipment ID.", nameof(shipmentId));

            return await _dbContext.Shipments
                .AsNoTracking()
                .Include(s => s.Order)
                .Include(s => s.ShipmentStatus)
                .Include(s => s.ShipmentItems)
                .FirstOrDefaultAsync(s => s.Id == shipmentId && !s.IsDeleted);
        }

        public async Task<Shipment?> UpdateAsync(Shipment shipment, string? changedBy = null, string? remarks = null, string? location = null)
        {
            if (shipment == null)
                throw new ArgumentNullException(nameof(shipment));

            var existing = await _dbContext.Shipments
                .FirstOrDefaultAsync(s => s.Id == shipment.Id && !s.IsDeleted);

            if (existing == null)
                return null;

            var currentStatus = (ShipmentStatusEnum)existing.ShipmentStatusId;
            var newStatus = (ShipmentStatusEnum)shipment.ShipmentStatusId;

            if (currentStatus != newStatus)
            {
                if (!AllowedTransitions.TryGetValue(currentStatus, out var validNext) || !validNext.Contains(newStatus))
                    throw new InvalidOperationException($"Invalid status transition from {currentStatus} to {newStatus}.");

                // Update shipment status
                existing.ShipmentStatusId = shipment.ShipmentStatusId;

                // Add status history record
                var history = new ShipmentStatusHistory
                {
                    Id = Guid.NewGuid(),
                    ShipmentId = existing.Id,
                    OldStatusId = (int)currentStatus,
                    NewStatusId = (int)newStatus,
                    ChangedBy = changedBy,
                    ChangedAt = DateTime.UtcNow,
                    Remarks = remarks,
                    Location = location
                };

                await _dbContext.ShipmentStatusHistories.AddAsync(history);
            }

            // Update other fields
            existing.CarrierName = shipment.CarrierName;
            existing.TrackingNumber = shipment.TrackingNumber;
            existing.EstimatedDeliveryDate = shipment.EstimatedDeliveryDate;
            existing.DeliveredAt = shipment.DeliveredAt;
            existing.UpdatedAt = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync();

            return existing;
        }

        //public async Task<Shipment?> GetByTrackingNumberAsync(string TrackingNumber)
        //{
        //    var shipment = await _dbContext.Shipments
        //        .AsNoTracking()
        //        .Include(s => s.ShipmentStatus)
        //        .Include(s => s.ShipmentItems)
        //        .Include(s => s.ShipmentStatusHistories)
        //        .FirstOrDefaultAsync(s => s.TrackingNumber == TrackingNumber);

        //    if (shipment != null)
        //    {
        //        // Order the status histories after retrieval
        //        shipment.ShipmentStatusHistories = shipment.ShipmentStatusHistories
        //            .OrderBy(h => h.ChangedAt)
        //            .ToList();
        //    }

        //    return shipment;
        //}

        public async Task<Shipment?> GetByTrackingNumberAsync(string trackingNumber)
        {
            return await _dbContext.Shipments
                .AsNoTracking()
                .Include(s => s.ShipmentStatus)
                .Include(s => s.ShipmentItems)
                .Include(s => s.ShipmentStatusHistories.OrderBy(h => h.ChangedAt))
                .FirstOrDefaultAsync(s => s.TrackingNumber == trackingNumber);
        }
    }
}
