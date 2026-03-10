using APIGateway.GraphQL.Output.Models;
namespace APIGateway.GraphQL.Output.Types
{
    // GraphQL Output Type mapping for PlaceOrderPayload.
    // Why do we return a "payload" object in GraphQL mutations?
    // - It gives the client a predictable response shape for both success and failure.
    // - Instead of throwing exceptions for validation/business failures,
    //   we can return:
    //   1) Order   -> when operation succeeds
    //   2) Errors  -> when operation fails (or empty when success)
    public class PlaceOrderPayloadType : ObjectType<PlaceOrderPayload>
    {
        protected override void Configure(IObjectTypeDescriptor<PlaceOrderPayload> descriptor)
        {
            // Name shown in GraphQL schema as: type PlaceOrderPayload { ... }
            descriptor.Name("PlaceOrderPayload");

            // 'Order' is nullable because placeOrder can fail.
            // On failure:
            // - Order will be null
            // - Errors will contain one or more UserError entries
            //
            // On success:
            // - Order will contain created order details
            // - Errors will typically be empty
            descriptor.Field(x => x.Order)
                      .Type<OrderOutputType>();

            // 'Errors' is a NonNull list of NonNull UserError objects:
            // - The list itself is never null (client can safely iterate without null checks).
            // - Each error item inside the list is never null.
            //
            // This design makes UI handling simple:
            // if (errors.length > 0) show errors;
            descriptor.Field(x => x.Errors)
                      .Type<NonNullType<ListType<NonNullType<UserErrorType>>>>();
        }
    }
}
