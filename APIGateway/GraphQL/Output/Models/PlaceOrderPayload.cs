using OrderService.Application.DTOs.Order;
namespace APIGateway.GraphQL.Output.Models
{
    // Represents the GraphQL mutation response for "placeOrder".
    // Instead of returning only a boolean or only an ID,
    // return a payload that contains:
    // - The successful result (Order)
    // - Any errors (Errors)
    public class PlaceOrderPayload
    {
        // The successful order response returned by OrderService after placing the order.
        // This will be null when the operation fails.
        // Example: OrderId, OrderStatus, TotalAmount, CreatedOn, etc.
        public OrderResponseDTO? Order { get; set; }

        // List of user-friendly errors.
        // This list will be empty when the mutation succeeds.
        // If the mutation fails, this list can contain one or multiple errors,
        // depending on the validation/business rules.
        public List<UserError> Errors { get; set; } = new();
    }
}
