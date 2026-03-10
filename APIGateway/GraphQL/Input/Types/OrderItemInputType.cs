using APIGateway.GraphQL.Input.Models;
namespace APIGateway.GraphQL.Input.Types
{
    // GraphQL Input Type definition for OrderItemInputModel.
    // GraphQL does not directly use C# models,
    // so we map the model to a GraphQL Input Type.

    // Important:
    // - InputModel = C# representation (used in .NET code)
    // - InputType  = GraphQL schema representation (what the client sees/uses)
    // 
    // This class tells Hot Chocolate how to expose the input model in the GraphQL schema.
    public class OrderItemInputType : InputObjectType<OrderItemInputModel>
    {
        // Configure the GraphQL schema for this input object.
        // The descriptor allows us to:
        // - Set the input type name shown to clients
        // - Control field types (UUID, Int, NonNull)
        // - Add constraints and schema-level rules
        protected override void Configure(IInputObjectTypeDescriptor<OrderItemInputModel> descriptor)
        {
            // The name that will appear in the GraphQL schema.
            // Clients will use this name inside mutations like:
            // items: [OrderItemInput!]!
            descriptor.Name("OrderItemInput");

            // Map ProductId property to a GraphQL field named "productId".
            // NonNullType means the client MUST provide this field.
            // UuidType ensures GraphQL treats it as a UUID/GUID value.
            descriptor.Field(x => x.ProductId)
                      .Type<NonNullType<UuidType>>();

            // Map Quantity property to a GraphQL field named "quantity".
            // NonNullType forces client to send the quantity.
            // IntType ensures it is treated as an integer at schema level.
            // Note: Range validation (>=1) is still handled by DataAnnotations validation.
            descriptor.Field(x => x.Quantity)
                      .Type<NonNullType<IntType>>();
        }
    }
}
