namespace APIGateway.GraphQL.Output.Models
{
    // Represents a user-friendly error that can be returned to the client
    // as part of a GraphQL mutation response.

    // Example usage:
    // - Code: "VALIDATION_ERROR", "NOT_FOUND", "PAYMENT_FAILED", "OUT_OF_STOCK"
    // - Message: A clean message to show on UI.
    public class UserError
    {
        // A short machine-readable error code.
        // The frontend can use this code to decide what to show or what action to take
        // (e.g., highlight field, show toast, redirect).
        // Default value is VALIDATION_ERROR for generic invalid inputs.
        public string Code { get; set; } = "VALIDATION_ERROR";

        // A human-readable message that can be shown directly on UI.
        // Keep it simple, safe, and user-friendly (avoid technical details).
        public string Message { get; set; } = "Invalid request.";
    }
}
