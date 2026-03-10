using APIGateway.DTOs.OrderSummary;
namespace APIGateway.GraphQL.Output.Types
{
    // GraphQL Output Type for customer/user information shown on the Order Details screen.
    // Customer info can vary by screen:
    // - Some screens need full profile details (name, email, mobile)
    // - Some screens may need only UserId + FullName
    // GraphQL makes this easy because clients can request only the fields they need.
    public class CustomerInfoOutputType : ObjectType<CustomerInfoDTO>
    {
        protected override void Configure(IObjectTypeDescriptor<CustomerInfoDTO> descriptor)
        {
            // Name shown in schema as:
            descriptor.Name("CustomerInfo");

            // UserId is required because it's the key identity of the customer.
            descriptor.Field(x => x.UserId).Type<NonNullType<UuidType>>();

            // These fields are optional because user profile data may not always exist
            // (example: email/mobile not provided, profile photo not set).
            descriptor.Field(x => x.FullName).Type<StringType>();
            descriptor.Field(x => x.Email).Type<StringType>();
            descriptor.Field(x => x.Mobile).Type<StringType>();
            descriptor.Field(x => x.ProfilePhotoUrl).Type<StringType>();
        }
    }
}

