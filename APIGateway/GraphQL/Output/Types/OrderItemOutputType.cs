using OrderService.Application.DTOs.Order;
using OrderService.Domain.Enums;
namespace APIGateway.GraphQL.Output.Types
{
    // GraphQL Output Type mapping for OrderItemResponseDTO.
    // - OrderItemResponseDTO is a C# DTO coming from OrderService.
    // - OrderItemOutputType defines how that DTO appears in the GraphQL schema.
    public class OrderItemOutputType : ObjectType<OrderItemResponseDTO>
    {
        // The descriptor helps us control:
        // - GraphQL type name ("OrderItem")
        // - Field types (UUID, String, Int, Decimal, Enum)
        // - Which fields are required (NonNull) vs optional (nullable)
        protected override void Configure(IObjectTypeDescriptor<OrderItemResponseDTO> descriptor)
        {
            // This is the name clients will see in the GraphQL schema
            descriptor.Name("OrderItem");

            // Unique identifier for this order item (line item).
            // Marked NonNull because the UI should always receive a valid identifier.
            descriptor.Field(x => x.OrderItemId)
                      .Type<NonNullType<UuidType>>();

            // Product identifier for the item.
            // NonNull because each order item must be linked to a product.
            descriptor.Field(x => x.ProductId)
                      .Type<NonNullType<UuidType>>();

            // ProductName is typically displayed on the UI (order details page).
            // NonNull ensures clients don't have to handle missing product names.
            descriptor.Field(x => x.ProductName)
                      .Type<NonNullType<StringType>>();

            // ItemStatusId represents the current status of the individual item (e.g., Placed/Shipped/Delivered).
            // Exposed as a GraphQL enum so client gets a fixed list of allowed values.
            // NonNull ensures UI always receives the status.
            descriptor.Field(x => x.ItemStatusId)
                      .Type<NonNullType<EnumType<OrderStatusEnum>>>();

            // Alternative (Hot Chocolate can infer enums automatically),
            // but explicit mapping makes schema intention clearer:
            // descriptor.Field(x => x.ItemStatusId);

            // Price information at the time of purchase.
            // NonNull because pricing is critical for invoice/order display.
            descriptor.Field(x => x.PriceAtPurchase)
                      .Type<NonNullType<DecimalType>>();

            // DiscountedPrice is included so UI can show MRP vs discounted pricing.
            descriptor.Field(x => x.DiscountedPrice)
                      .Type<NonNullType<DecimalType>>();

            // Quantity ordered.
            descriptor.Field(x => x.Quantity)
                      .Type<NonNullType<IntType>>();

            // TotalPrice = DiscountedPrice * Quantity (or final calculated price for this item).
            // NonNull because totals are needed for summary calculations in UI.
            descriptor.Field(x => x.TotalPrice)
                      .Type<NonNullType<DecimalType>>();
        }
    }
}
