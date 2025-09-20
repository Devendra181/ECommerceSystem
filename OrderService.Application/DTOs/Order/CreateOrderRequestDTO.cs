using OrderService.Contracts.DTOs;
using OrderService.Contracts.Enums;
using System.ComponentModel.DataAnnotations;

namespace OrderService.Application.DTOs.Order
{
    public class CreateOrderRequestDTO
    {
        [Required(ErrorMessage = "UserId is required.")]
        public Guid UserId { get; set; }

        [Required(ErrorMessage = "At least one order item is required.")]
        [MinLength(1, ErrorMessage = "At least one order item is required.")]
        public List<OrderItemRequestDTO> Items { get; set; } = new();

        public Guid? ShippingAddressId { get; set; }
        public AddressDTO? ShippingAddress { get; set; }

        public Guid? BillingAddressId { get; set; }
        public AddressDTO? BillingAddress { get; set; }

        [Required(ErrorMessage = "PaymentMethod is required.")]
        // public string PaymentMethod { get; set; } = null!;
        public PaymentMethodEnum PaymentMethod { get; set; }
    }
}
