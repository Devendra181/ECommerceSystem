using APIGateway.DTOs.OrderSummary;
namespace APIGateway.GraphQL.Output.Types
{
    // GraphQL Output Type for product/item information inside an order.
    // This type represents the "Items" section of an Order Details screen.
    // It includes product identity + display info (name, image) + pricing and quantity.
    public class OrderProductInfoOutputType : ObjectType<OrderProductInfoDTO>
    {
        protected override void Configure(IObjectTypeDescriptor<OrderProductInfoDTO> descriptor)
        {
            // Name shown in schema as:
            descriptor.Name("OrderProductInfo");

            // ProductId and Name are required because every line item must refer to a product,
            // and UI must show at least the product name.
            descriptor.Field(x => x.ProductId).Type<NonNullType<UuidType>>();
            descriptor.Field(x => x.Name).Type<NonNullType<StringType>>();

            // SKU and ImageUrl are optional because:
            // - SKU might not be configured for all products
            // - ImageUrl might be missing for some products
            descriptor.Field(x => x.SKU).Type<StringType>();
            descriptor.Field(x => x.ImageUrl).Type<StringType>();

            // Quantity and UnitPrice are required for displaying order item breakdown.
            descriptor.Field(x => x.Quantity).Type<NonNullType<IntType>>();
            descriptor.Field(x => x.UnitPrice).Type<NonNullType<DecimalType>>();

            // LineTotal is typically a computed value:
            // LineTotal = Quantity * UnitPrice (or after discount rules if applied)
            // Mark NonNull because UI expects it for totals.
            descriptor.Field(x => x.LineTotal).Type<NonNullType<DecimalType>>();
        }
    }
}
