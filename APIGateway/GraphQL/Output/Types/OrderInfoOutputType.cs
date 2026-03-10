using APIGateway.DTOs.OrderSummary;
namespace APIGateway.GraphQL.Output.Types
{
    // GraphQL Output Type for the order-level summary information.
    // This type is typically used in an "Order Details / Order Summary" screen where the UI needs:
    // - Order number + date + status
    // - Pricing breakdown (subtotal, discount, shipping, tax, total)
    // - Currency + payment method
    public class OrderInfoOutputType : ObjectType<OrderInfoDTO>
    {
        protected override void Configure(IObjectTypeDescriptor<OrderInfoDTO> descriptor)
        {
            // Name shown in GraphQL schema as:
            descriptor.Name("OrderInfo");

            // Basic order details are required for any order summary.
            descriptor.Field(x => x.OrderNumber).Type<NonNullType<StringType>>();
            descriptor.Field(x => x.OrderDate).Type<NonNullType<DateTimeType>>();
            descriptor.Field(x => x.Status).Type<NonNullType<StringType>>();

            // Pricing values are required because UI needs them to display totals and invoice-like summary.
            descriptor.Field(x => x.SubTotalAmount).Type<NonNullType<DecimalType>>();
            descriptor.Field(x => x.DiscountAmount).Type<NonNullType<DecimalType>>();
            descriptor.Field(x => x.ShippingCharges).Type<NonNullType<DecimalType>>();
            descriptor.Field(x => x.TaxAmount).Type<NonNullType<DecimalType>>();
            descriptor.Field(x => x.TotalAmount).Type<NonNullType<DecimalType>>();

            // Currency and payment method are usually required to display on the order summary screen.
            // (Example: INR / UPI / COD)
            descriptor.Field(x => x.Currency).Type<NonNullType<StringType>>();
            descriptor.Field(x => x.PaymentMethod).Type<NonNullType<StringType>>();
        }
    }
}
