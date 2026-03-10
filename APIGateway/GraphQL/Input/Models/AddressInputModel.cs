using System.ComponentModel.DataAnnotations;
namespace APIGateway.GraphQL.Input.Models
{
    // Represents an address object coming from the client in a GraphQL request.
    // This is a pure C# input model (DTO) used by GraphQL mutations/inputs.
    public class AddressInputModel
    {
        // Optional Id.
        // - If Id is provided, it usually means "update existing address".
        // - If Id is null, it usually means "create a new address".
        public Guid? Id { get; set; }

        // Address line 1 is mandatory (example: house no, street, area).
        // [Required] ensures client must send a value.
        // [MaxLength] prevents very large strings that could cause DB/storage issues.
        [Required(ErrorMessage = "AddressLine1 is required.")]
        [MaxLength(200, ErrorMessage = "AddressLine1 must be <= 200 characters.")]
        public string AddressLine1 { get; set; } = null!;

        // Address line 2 is optional (landmark, apartment, etc.).
        // MaxLength still applies to prevent overly long input.
        [MaxLength(200, ErrorMessage = "AddressLine2 must be <= 200 characters.")]
        public string? AddressLine2 { get; set; }

        // City is mandatory.
        // [MaxLength] keeps the input clean and safe (also helps DB consistency).
        [Required(ErrorMessage = "City is required.")]
        [MaxLength(100, ErrorMessage = "City must be <= 100 characters.")]
        public string City { get; set; } = null!;

        // State is mandatory.
        [Required(ErrorMessage = "State is required.")]
        [MaxLength(100, ErrorMessage = "State must be <= 100 characters.")]
        public string State { get; set; } = null!;

        // PostalCode is mandatory.
        // MaxLength is limited because postal codes are typically short (PIN/ZIP).
        [Required(ErrorMessage = "PostalCode is required.")]
        [MaxLength(20, ErrorMessage = "PostalCode must be <= 20 characters.")]
        public string PostalCode { get; set; } = null!;

        // Country is mandatory.
        [Required(ErrorMessage = "Country is required.")]
        [MaxLength(100, ErrorMessage = "Country must be <= 100 characters.")]
        public string Country { get; set; } = null!;

        // If true, this address should be treated as the customer's default billing address.
        public bool IsDefaultBilling { get; set; }

        // If true, this address should be treated as the customer's default shipping address.
        public bool IsDefaultShipping { get; set; }
    }
}
