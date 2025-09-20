using PaymentService.Domain.Enums;
namespace PaymentService.Domain.MasterEntities
{
    public class TransactionStatusMaster
    {
        public int Id { get; set; }
        public TransactionStatusEnum Name { get; set; }
        public string? Description { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime? UpdatedOn { get; set; }
    }
}
