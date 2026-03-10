using System.ComponentModel.DataAnnotations;
namespace APIGateway.GraphQL.Input.Models
{
    // Represents a single cart/order item coming from the client.
    // This is a plain C# input model (DTO) used by our GraphQL mutations.
    // It is similar to a DTO used in REST APIs.
    public class OrderItemInputModel
    {
        // ProductId is mandatory.
        // If the client does not send this value,
        // GraphQL validation will fail before execution.
        [Required(ErrorMessage = "ProductId is required.")]
        public Guid ProductId { get; set; }

        // Quantity must be at least 1.
        // This prevents invalid orders such as 0 or negative quantities.
        // Validation happens before calling backend services.

        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1.")]
        public int Quantity { get; set; }
    }
}

