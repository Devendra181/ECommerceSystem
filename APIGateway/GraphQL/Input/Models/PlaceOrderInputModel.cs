using OrderService.Contracts.Enums;
using System.ComponentModel.DataAnnotations;
namespace APIGateway.GraphQL.Input.Models
{
    // Represents the input payload sent by the client when placing an order via GraphQL.
    // - The client sends cart items + user info + address info + payment method in ONE request.
    // - The gateway validates the input and then forwards/orchestrates the actual order creation.
    public class PlaceOrderInputModel
    {
        // The user placing the order.
        // Required because an order must always belong to a user/customer.
        [Required(ErrorMessage = "UserId is required.")]
        public Guid UserId { get; set; }

        // The list of items the user is ordering (cart items). 
        // [Required] + [MinLength(1)] ensures the client cannot place an order with an empty cart.
        // This prevents unnecessary calls to backend services.
        [Required(ErrorMessage = "At least one order item is required.")]
        [MinLength(1, ErrorMessage = "At least one order item is required.")]
        public List<OrderItemInputModel> Items { get; set; } = new();

        // Optional reference to an already saved shipping address.
        // If this is provided, the client is saying: "Use my existing saved address".
        public Guid? ShippingAddressId { get; set; }

        // Optional full shipping address object.
        // If this is provided, the client is saying: "Use this new shipping address right now".
        // In many real projects, UI sends either ShippingAddressId OR ShippingAddress.
        // This model allows both, and business logic can enforce the rule if needed.
        public AddressInputModel? ShippingAddress { get; set; }

        // Optional reference to an already saved billing address.
        // Similar usage as ShippingAddressId.
        public Guid? BillingAddressId { get; set; }

        // Optional full billing address object.
        // Similar usage as ShippingAddress.
        public AddressInputModel? BillingAddress { get; set; }

        // Payment method chosen by the client (e.g., COD, Card, UPI, NetBanking, etc.).
        // Required because we must know how the customer is paying before placing the order.
        [Required(ErrorMessage = "PaymentMethod is required.")]
        public PaymentMethodEnum PaymentMethod { get; set; }
    }
}
