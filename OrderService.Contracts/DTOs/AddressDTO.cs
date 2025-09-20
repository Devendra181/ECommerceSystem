namespace OrderService.Contracts.DTOs
{
    public class AddressDTO
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string AddressLine1 { get; set; } = null!;
        public string? AddressLine2 { get; set; }
        public string City { get; set; } = null!;
        public string State { get; set; } = null!;
        public string PostalCode { get; set; } = null!;
        public string Country { get; set; } = null!;
        public bool IsDefaultBilling { get; set; }
        public bool IsDefaultShipping { get; set; }
    }
}
