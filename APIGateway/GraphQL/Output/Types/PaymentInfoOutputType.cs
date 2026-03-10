using APIGateway.DTOs.OrderSummary;
namespace APIGateway.GraphQL.Output.Types
{
    // GraphQL Output Type for payment information related to an order.
    // Payment details may vary depending on payment method:
    // - COD: PaymentId/transaction may not exist, PaidOn may be null
    // - Online payment: PaymentId, PaidOn, and transaction reference often exist
    public class PaymentInfoOutputType : ObjectType<PaymentInfoDTO>
    {
        protected override void Configure(IObjectTypeDescriptor<PaymentInfoDTO> descriptor)
        {
            // Name shown in schema as:
            // type PaymentInfo { ... }
            descriptor.Name("PaymentInfo");

            // PaymentId can be null in some flows (example: COD or payment not created yet),
            // so it is kept nullable in schema.
            descriptor.Field(x => x.PaymentId).Type<UuidType>();

            // Status and Method are mandatory for UI (Paid/Pending/Failed, COD/UPI/Card).
            descriptor.Field(x => x.Status).Type<NonNullType<StringType>>();
            descriptor.Field(x => x.Method).Type<NonNullType<StringType>>();

            // PaidOn is optional because payment may not be completed yet.
            descriptor.Field(x => x.PaidOn).Type<DateTimeType>();

            // TransactionReference is optional because:
            // - COD may not have it
            // - Some gateways may provide it only after success
            descriptor.Field(x => x.TransactionReference).Type<StringType>();
        }
    }
}
