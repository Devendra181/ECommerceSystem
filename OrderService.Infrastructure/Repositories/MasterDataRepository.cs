using Microsoft.EntityFrameworkCore;
using OrderService.Domain.Entities;
using OrderService.Domain.Enums;
using OrderService.Domain.Repositories;
using OrderService.Infrastructure.Persistence;

namespace OrderService.Infrastructure.Repositories
{
    public class MasterDataRepository : IMasterDataRepository
    {
        private readonly OrderDbContext _dbContext;

        public MasterDataRepository(OrderDbContext dbContext)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        public async Task<List<OrderStatus>> GetOrderStatusesAsync()
        {
            return await _dbContext.OrderStatuses.AsNoTracking().ToListAsync();
        }

        public async Task<OrderStatus?> GetOrderStatusByIdAsync(int id)
        {
            return await _dbContext.OrderStatuses.AsNoTracking().FirstOrDefaultAsync(o => o.Id == id);
        }

        public async Task<List<CancellationStatus>> GetCancellationStatusesAsync()
        {
            return await _dbContext.CancellationStatuses.AsNoTracking().ToListAsync();
        }

        public async Task<CancellationStatus?> GetCancellationStatusByIdAsync(int id)
        {
            return await _dbContext.CancellationStatuses.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<List<ReturnStatus>> GetReturnStatusesAsync()
        {
            return await _dbContext.ReturnStatuses.AsNoTracking().ToListAsync();
        }

        public async Task<ReturnStatus?> GetReturnStatusByIdAsync(int id)
        {
            return await _dbContext.ReturnStatuses.AsNoTracking().FirstOrDefaultAsync(r => r.Id == id);
        }

        public async Task<List<RefundStatus>> GetRefundStatusesAsync()
        {
            return await _dbContext.RefundStatuses.AsNoTracking().ToListAsync();
        }

        public async Task<RefundStatus?> GetRefundStatusByIdAsync(int id)
        {
            return await _dbContext.RefundStatuses.AsNoTracking().FirstOrDefaultAsync(r => r.Id == id);
        }

        public async Task<List<ShipmentStatus>> GetShipmentStatusesAsync()
        {
            return await _dbContext.ShipmentStatuses.AsNoTracking().ToListAsync();
        }

        public async Task<ShipmentStatus?> GetShipmentStatusByIdAsync(int id)
        {
            return await _dbContext.ShipmentStatuses.AsNoTracking().FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task<List<string>> GetReasonTypesAsync()
        {
            return await _dbContext.ReasonMasters
               .AsNoTracking()
               .Select(r => r.ReasonType.ToString())
               .Distinct()
               .ToListAsync();
        }

        public async Task<List<ReasonMaster>> GetReasonsByTypeAsync(ReasonTypeEnum reasonType)
        {
            return await _dbContext.ReasonMasters
                .AsNoTracking()
                .Where(r => r.ReasonType == reasonType)
                .OrderBy(r => r.Id)
                .ToListAsync();
        }

        public async Task<List<CancellationPolicy>> GetCancellationPoliciesAsync()
        {
            return await _dbContext.CancellationPolicies.AsNoTracking().ToListAsync();
        }

        public async Task<CancellationPolicy?> GetCancellationPolicyByIdAsync(int id)
        {
            return await _dbContext.CancellationPolicies.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<CancellationPolicy?> GetActiveCancellationPolicyAsync()
        {
            var now = DateTime.UtcNow;
            return await _dbContext.CancellationPolicies
                .AsNoTracking()
                .Where(c => c.IsActive && c.CreatedAt <= now)
                .OrderByDescending(c => c.CreatedAt)
                .FirstOrDefaultAsync();
        }

        public async Task<List<ReturnPolicy>> GetReturnPoliciesAsync()
        {
            return await _dbContext.ReturnPolicies.AsNoTracking().ToListAsync();
        }

        public async Task<ReturnPolicy?> GetReturnPolicyByIdAsync(int id)
        {
            return await _dbContext.ReturnPolicies.AsNoTracking().FirstOrDefaultAsync(r => r.Id == id);
        }

        public async Task<ReturnPolicy?> GetActiveReturnPolicyAsync()
        {
            var now = DateTime.UtcNow;
            return await _dbContext.ReturnPolicies
                .AsNoTracking()
                .Where(r => r.IsActive && r.CreatedAt <= now)
                .OrderByDescending(r => r.CreatedAt)
                .FirstOrDefaultAsync();
        }

        public async Task<List<Discount>> GetDiscountsAsync()
        {
            return await _dbContext.Discounts.AsNoTracking().ToListAsync();
        }

        public async Task<Discount?> GetDiscountByIdAsync(int id)
        {
            return await _dbContext.Discounts.AsNoTracking().FirstOrDefaultAsync(d => d.Id == id);
        }

        public async Task<List<Discount>> GetActiveDiscountsAsync(DateTime asOfDate)
        {
            return await _dbContext.Discounts
                .AsNoTracking()
                .Where(d => d.IsActive && d.StartDate <= asOfDate && d.EndDate >= asOfDate)
                .ToListAsync();
        }

        public async Task<List<Tax>> GetTaxesAsync()
        {
            return await _dbContext.Taxes.AsNoTracking().ToListAsync();
        }

        public async Task<Tax?> GetTaxByIdAsync(int id)
        {
            return await _dbContext.Taxes.AsNoTracking().FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task<List<Tax>> GetActiveTaxesAsync(DateTime asOfDate)
        {
            return await _dbContext.Taxes
                .AsNoTracking()
                .Where(t => t.IsActive && t.ValidFrom <= asOfDate)
                .ToListAsync();
        }
    }
}

