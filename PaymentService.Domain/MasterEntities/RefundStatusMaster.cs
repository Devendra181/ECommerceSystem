using PaymentService.Domain.Enums;
namespace PaymentService.Domain.MasterEntities
{
    public class RefundStatusMaster
    {
        public int Id { get; set; }
        public RefundStatusEnum Name { get; set; }
        public string? Description { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime? UpdatedOn { get; set; }
    }
}
