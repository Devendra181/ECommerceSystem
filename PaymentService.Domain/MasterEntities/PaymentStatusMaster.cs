using PaymentService.Domain.Enums;
namespace PaymentService.Domain.MasterEntities
{
    public class PaymentStatusMaster
    {
        public int Id { get; set; }
        public PaymentStatusEnum Name { get; set; }
        public string? Description { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime? UpdatedOn { get; set; }
    }
}
