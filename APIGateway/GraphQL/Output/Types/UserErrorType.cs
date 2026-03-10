using APIGateway.GraphQL.Output.Models;
namespace APIGateway.GraphQL.Output.Types
{
    // GraphQL output type mapping for the UserError model.
    // - UserError is a C# model used in our code.
    // - UserErrorType defines how that model appears in the GraphQL schema.
    // This allows us to control:
    // - The GraphQL type name ("UserError")
    // - Field nullability (NonNull fields)
    // - Field types (String, etc.)
    public class UserErrorType : ObjectType<UserError>
    {
        // Configure the schema representation of UserError.
        // GraphQL schema will expose:
        protected override void Configure(IObjectTypeDescriptor<UserError> descriptor)
        {
            // The name shown in GraphQL schema.
            // The client will see it as "UserError".
            descriptor.Name("UserError");

            // Code is required in the response because the UI can use it for logic:
            // e.g., show specific message, highlight a field, or display a toast.
            descriptor.Field(x => x.Code)
                      .Type<NonNullType<StringType>>();

            // Message is required because it is typically displayed directly to the user.
            // Keeping it NonNull ensures the client always receives a readable message.
            descriptor.Field(x => x.Message)
                      .Type<NonNullType<StringType>>();
        }
    }
}

