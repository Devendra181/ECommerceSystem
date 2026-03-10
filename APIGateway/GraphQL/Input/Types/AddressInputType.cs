using APIGateway.GraphQL.Input.Models;
namespace APIGateway.GraphQL.Input.Types
{
    // GraphQL Input Type definition for AddressInputModel.
    // Important distinction:
    // - AddressInputModel = C# class used inside .NET code (DTO)
    // - AddressInputType  = GraphQL schema definition exposed to clients

    // This class controls:
    // - The GraphQL input object name
    // - Which fields are exposed
    // - Which fields are required (NonNull)
    // - The exact GraphQL scalar types (String, UUID, Boolean, etc.)
    public class AddressInputType : InputObjectType<AddressInputModel>
    {
        // Configure how this input appears in the GraphQL schema.
        // The descriptor lets us customize input name and field types.
        protected override void Configure(IInputObjectTypeDescriptor<AddressInputModel> descriptor)
        {
            // The name of the input object in GraphQL schema.
            // Client will use it like:
            // address: AddressInput!
            descriptor.Name("AddressInput");

            // Id is optional, so we don't wrap it with NonNullType<>.
            // This allows the client to omit it for "create new address".
            descriptor.Field(x => x.Id)
                      .Type<UuidType>();

            // AddressLine1 is required for the address to be meaningful,
            // so we mark it NonNull in the GraphQL schema.
            // If missing, GraphQL rejects the request immediately.
            descriptor.Field(x => x.AddressLine1)
                      .Type<NonNullType<StringType>>();

            // AddressLine2 is optional, so we keep it nullable in schema.
            descriptor.Field(x => x.AddressLine2)
                      .Type<StringType>();

            // City/State/PostalCode/Country are required, so use NonNullType<StringType>.
            descriptor.Field(x => x.City)
                      .Type<NonNullType<StringType>>();

            descriptor.Field(x => x.State)
                      .Type<NonNullType<StringType>>();

            descriptor.Field(x => x.PostalCode)
                      .Type<NonNullType<StringType>>();

            descriptor.Field(x => x.Country)
                      .Type<NonNullType<StringType>>();

            // These flags are booleans. We mark them NonNull so client always sends true/false.
            // This avoids ambiguity and makes the contract explicit.
            descriptor.Field(x => x.IsDefaultBilling)
                      .Type<NonNullType<BooleanType>>();

            descriptor.Field(x => x.IsDefaultShipping)
                      .Type<NonNullType<BooleanType>>();
        }
    }
}
