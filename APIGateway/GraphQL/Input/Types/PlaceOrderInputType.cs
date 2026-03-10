using APIGateway.GraphQL.Input.Models;
using OrderService.Contracts.Enums;
namespace APIGateway.GraphQL.Input.Types
{
    // GraphQL Input Type definition for PlaceOrderInputModel.
    // - PlaceOrderInputModel is a C# DTO used internally.
    // - PlaceOrderInputType defines how that DTO appears in the GraphQL schema.

    // This helps us control:
    // - Input object name in schema
    // - Which fields are required (NonNull)
    // - How nested inputs (items, addresses) are represented
    // - How enums are exposed to clients
    public class PlaceOrderInputType : InputObjectType<PlaceOrderInputModel>
    {
        // Configures the GraphQL schema details for the "PlaceOrderInput" object.
        // The descriptor allows us to map each C# property to a GraphQL field and type.
        protected override void Configure(IInputObjectTypeDescriptor<PlaceOrderInputModel> descriptor)
        {
            // Name of the input object shown in GraphQL schema.
            descriptor.Name("PlaceOrderInput");

            // UserId is mandatory, so we mark it as NonNull UUID in GraphQL schema.
            // If the client does not send it, GraphQL rejects the request immediately.
            descriptor.Field(x => x.UserId)
                      .Type<NonNullType<UuidType>>();

            // Items is mandatory and must contain at least one item.
            // GraphQL-level: NonNull list + NonNull items inside it.
            // That means:
            // - Items list itself cannot be null
            // - Each item inside list cannot be null
            descriptor.Field(x => x.Items)
                      .Type<NonNullType<ListType<NonNullType<OrderItemInputType>>>>();

            // ShippingAddressId is optional (nullable),
            // so we expose it as nullable UUID type in schema.
            descriptor.Field(x => x.ShippingAddressId)
                      .Type<UuidType>();

            // ShippingAddress is optional (nullable) because user may choose an existing saved address
            // via ShippingAddressId. If UI provides a new address, it sends this object.
            descriptor.Field(x => x.ShippingAddress)
                      .Type<AddressInputType>();

            // BillingAddressId is optional for the same reason as ShippingAddressId.
            descriptor.Field(x => x.BillingAddressId)
                      .Type<UuidType>();

            // BillingAddress is optional. UI can send it only when a new billing address is needed.
            descriptor.Field(x => x.BillingAddress)
                      .Type<AddressInputType>();

            // PaymentMethod is required, so we mark it NonNull.
            // EnumType<PaymentMethodEnum> tells Hot Chocolate to expose this C# enum as a GraphQL enum.
            // Clients will see a fixed set of allowed values in the schema.
            descriptor.Field(x => x.PaymentMethod)
                      .Type<NonNullType<EnumType<PaymentMethodEnum>>>();

            // Alternative (less explicit) approach:
            // descriptor.Field(x => x.PaymentMethod);
            // Hot Chocolate can infer enum type automatically, but we keep it explicit for clarity.
        }
    }
}
