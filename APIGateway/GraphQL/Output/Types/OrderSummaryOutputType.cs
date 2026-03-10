using APIGateway.DTOs.OrderSummary;
namespace APIGateway.GraphQL.Output.Types
{
    // GraphQL Output Type for OrderSummaryResponseDTO.
    // This is a "screen-ready" response model typically used for the Order Details screen.
    // Instead of the UI calling multiple microservices and merging responses,
    // the Gateway returns a single OrderSummary object that contains:
    // - Order info (totals, status, payment method, etc.)
    // - Customer info (optional, depends on availability/permissions)
    // - Products/items (line items)
    // - Payment info (optional, depends on payment flow)
    //
    // GraphQL advantage:
    // Different screens can request different shapes from this type.
    // Example:
    // - Mobile screen may request only OrderId + Status + Total
    // - Web screen may request deep product list + customer + payment details
    public class OrderSummaryOutputType : ObjectType<OrderSummaryResponseDTO>
    {
        protected override void Configure(IObjectTypeDescriptor<OrderSummaryResponseDTO> descriptor)
        {
            // Name shown in GraphQL schema as:
            descriptor.Name("OrderSummary");

            // Unique identifier of the order summary.
            // NonNull because it is the key used by UI and links/actions.
            descriptor.Field(x => x.OrderId)
                      .Type<NonNullType<UuidType>>();

            // Order info is mandatory because every order summary must contain base order details.
            // NonNull ensures the UI can safely read totals/status without null checks.
            descriptor.Field(x => x.Order)
                      .Type<NonNullType<OrderInfoOutputType>>();

            // Customer info is optional because:
            // - Some screens may not require it
            // - Some flows may not fetch it (or privacy rules may hide it)
            // Keeping it nullable makes the response flexible across screens/clients.
            descriptor.Field(x => x.Customer)
                      .Type<CustomerInfoOutputType>();

            // Products (items) list is mandatory because an order summary without items is meaningless.
            // NonNull list + NonNull items means:
            // - The list itself is never null (client can iterate safely)
            // - Each product entry is never null
            descriptor.Field(x => x.Products)
                      .Type<NonNullType<ListType<NonNullType<OrderProductInfoOutputType>>>>();

            // Payment info is optional because:
            // - COD may not have full payment details
            // - Online payment may not be completed yet
            // - Payment service might be temporarily unavailable in partial scenarios
            descriptor.Field(x => x.Payment)
                      .Type<PaymentInfoOutputType>();

            // Indicates whether this summary is partial.
            // Example: some microservice call failed, so only partial data is returned.
            // NonNull so the UI can always rely on this flag.
            descriptor.Field(x => x.IsPartial)
                      .Type<NonNullType<BooleanType>>();

            // Warnings are non-fatal messages meant for the UI.
            // Example:
            // - "Payment service unavailable, showing last known status."
            // - "Some product details could not be loaded."
            //
            // NonNull list ensures UI always receives a list (empty or filled).
            descriptor.Field(x => x.Warnings)
                      .Type<NonNullType<ListType<NonNullType<StringType>>>>();
        }
    }
}

