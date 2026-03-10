using OrderService.Application.DTOs.Order;
using OrderService.Contracts.Enums;
using OrderService.Domain.Enums;
namespace APIGateway.GraphQL.Output.Types
{
    // GraphQL Output Type mapping for OrderResponseDTO.
    // - This type represents the complete "Order" view that the UI can query.
    // - It is typically returned from:
    //   1) Order summary queries
    //   2) Place order mutation payload (PlaceOrderPayload.Order)
    //
    // GraphQL advantage:
    // - Clients can select only required fields (e.g., orderId + status)
    // - Or request deeper fields (e.g., items + pricing) based on screen needs.
    public class OrderOutputType : ObjectType<OrderResponseDTO>
    {
        protected override void Configure(IObjectTypeDescriptor<OrderResponseDTO> descriptor)
        {
            // Name of the GraphQL type visible to clients
            descriptor.Name("Order");

            // Core identifiers
            descriptor.Field(x => x.OrderId)
                      .Type<NonNullType<UuidType>>();

            // OrderNumber is usually shown to customer/admin as a readable identifier.
            descriptor.Field(x => x.OrderNumber)
                      .Type<NonNullType<StringType>>();

            // The user who placed the order.
            descriptor.Field(x => x.UserId)
                      .Type<NonNullType<UuidType>>();

            // Order status + payment method
            // These are enums, so GraphQL exposes them as enum values.
            // Clients get a strict set of allowed values which improves contract clarity.
            // Enums can be inferred automatically by Hot Chocolate,
            // but explicit mapping is clearer in training docs and real projects.
            descriptor.Field(x => x.OrderStatus)
                      .Type<NonNullType<EnumType<OrderStatusEnum>>>();

            descriptor.Field(x => x.PaymentMethod)
                      .Type<NonNullType<EnumType<PaymentMethodEnum>>>();

            // Date/time when the order was placed.
            // NonNull ensures UI can always display order placed date.
            descriptor.Field(x => x.OrderDate)
                      .Type<NonNullType<DateTimeType>>();

            // Order items (line items).
            // NonNull list + NonNull items means:
            // - The list itself is never null (client can safely iterate)
            // - Each item inside list is never null
            descriptor.Field(x => x.Items)
                      .Type<NonNullType<ListType<NonNullType<OrderItemOutputType>>>>();

            // Address references for shipping and billing.
            // These are NonNull because order processing requires both address references.
            descriptor.Field(x => x.ShippingAddressId)
                      .Type<NonNullType<UuidType>>();

            descriptor.Field(x => x.BillingAddressId)
                      .Type<NonNullType<UuidType>>();

            // Pricing summary fields
            // These are NonNull because totals are essential for order summary/invoice display.
            descriptor.Field(x => x.SubTotalAmount)
                      .Type<NonNullType<DecimalType>>();

            descriptor.Field(x => x.DiscountAmount)
                      .Type<NonNullType<DecimalType>>();

            descriptor.Field(x => x.ShippingCharges)
                      .Type<NonNullType<DecimalType>>();

            descriptor.Field(x => x.TaxAmount)
                      .Type<NonNullType<DecimalType>>();

            descriptor.Field(x => x.TotalAmount)
                      .Type<NonNullType<DecimalType>>();

            // Payment URL is optional.
            // Example: it may exist only for online payment methods (UPI/Card)
            // and might be null for COD or when payment is already completed.
            descriptor.Field(x => x.PaymentUrl)
                      .Type<StringType>();
        }
    }
}
