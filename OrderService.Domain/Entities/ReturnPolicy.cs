using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace OrderService.Domain.Entities
{
    public class ReturnPolicy
    {
        [Key]
        public int Id { get; set; }
        //Standard Return Policy, No Returns After 3 Days
        [Required, MaxLength(150)]
        public string PolicyName { get; set; } = null!;
        [MaxLength(1000)]
        public string? Description { get; set; }
        public int AllowedReturnDays { get; set; }
        [Column(TypeName = "decimal(5,2)")]
        public decimal RestockingFee { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ModifiedAt { get; set; }
    }
}
