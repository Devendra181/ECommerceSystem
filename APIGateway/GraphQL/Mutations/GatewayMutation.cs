using APIGateway.GraphQL.Input.Models;
using APIGateway.GraphQL.Output.Models;
using APIGateway.Services;
using HotChocolate.Authorization;
using OrderService.Application.DTOs.Order;
using OrderService.Contracts.DTOs;
namespace APIGateway.GraphQL.Mutations
{
    // GraphQL Mutation resolver class for the API Gateway.
    // Purpose:
    // - Exposes write operations (Mutations) through /graphql.
    // - Acts as a BFF layer: UI sends ONE request, gateway forwards it to the right microservice.
    public class GatewayMutation
    {
        // Mutation: placeOrder
        // Real-time UI scenario:
        // - Client sends cart items + userId + addresses + payment method in one request.
        // - Gateway forwards the request to OrderService (CreateOrder API).
        // - Gateway returns PlaceOrderPayload: { order, errors }

        [Authorize] // Only authenticated users can place orders.
        public async Task<PlaceOrderPayload> PlaceOrderAsync(
            PlaceOrderInputModel input,                         // GraphQL input sent by client
            [Service] IOrderApiClient orderApiClient,           // Gateway client that calls OrderService
            [Service] IHttpContextAccessor httpContextAccessor, // Needed to forward Authorization header
            CancellationToken cancellationToken)
        {
            // 1) Map GraphQL Input Model -> OrderService CreateOrderRequestDTO
            // GraphQL input models are designed for client-friendly inputs.
            // Microservice DTOs are designed for service contracts.
            // So we map between them in the gateway.
            var createOrderRequest = new CreateOrderRequestDTO
            {
                // Who is placing the order
                UserId = input.UserId,

                // How user wants to pay (Enum)
                PaymentMethod = input.PaymentMethod,

                // Address references (if UI selected saved addresses)
                ShippingAddressId = input.ShippingAddressId,
                BillingAddressId = input.BillingAddressId,

                // Convert item input list to OrderService expected request list
                Items = input.Items.Select(i => new OrderItemRequestDTO
                {
                    ProductId = i.ProductId,
                    Quantity = i.Quantity
                }).ToList(),

                // If UI provided a new address object (instead of addressId),
                // map it to AddressDTO for OrderService.
                ShippingAddress = MapAddress(input.UserId, input.ShippingAddress),
                BillingAddress = MapAddress(input.UserId, input.BillingAddress)
            };

            // 2) Forward Authorization header to downstream OrderService
            var authHeader = httpContextAccessor.HttpContext?.Request.Headers["Authorization"].ToString();

            // 3) Call downstream OrderService using gateway client
            // OrderService performs the actual business logic:
            // - validation rules
            // - pricing calculation
            // - inventory checks (if any)
            // - order creation
            DTOs.Common.ApiResponse<OrderResponseDTO> response =
                await orderApiClient.CreateOrderAsync(createOrderRequest, authHeader, cancellationToken);

            // 4) Convert downstream response to GraphQL payload
            // Success case:
            // - return Order data
            // - return empty error list
            if (response.Success && response.Data is not null)
            {
                return new PlaceOrderPayload
                {
                    Order = response.Data,
                    Errors = new List<UserError>() // always return list; keep it empty for success
                };
            }

            // Failure case:
            // We do NOT throw here because this is a business failure (not a system crash).
            // Instead, return errors in payload so UI can show a proper message.
            var errors = new List<UserError>();

            // If OrderService returned a top-level message, convert it to UserError.
            if (!string.IsNullOrWhiteSpace(response.Message))
            {
                errors.Add(new UserError
                {
                    Code = "ORDER_FAILED",
                    Message = response.Message!
                });
            }

            // If OrderService returned multiple error messages, convert each to UserError.
            if (response.Errors is not null && response.Errors.Count > 0)
            {
                errors.AddRange(response.Errors.Select(e => new UserError
                {
                    Code = "ORDER_FAILED",
                    Message = e
                }));
            }

            // Safety fallback: Ensure at least one error exists.
            if (errors.Count == 0)
            {
                errors.Add(new UserError
                {
                    Code = "ORDER_FAILED",
                    Message = "Failed to place order."
                });
            }

            // Return predictable mutation result: Order = null, Errors = list.
            return new PlaceOrderPayload { Order = null, Errors = errors };
        }

        // Maps a GraphQL AddressInputModel to the AddressDTO expected by OrderService.
        // - GraphQL input is designed for UI
        // - OrderService contract expects AddressDTO
        // - Mapping keeps the gateway clean and avoids repeating code in the mutation
        private static AddressDTO? MapAddress(Guid userId, AddressInputModel? address)
        {
            // If user did not provide a new address object, skip mapping.
            if (address is null) return null;

            return new AddressDTO
            {
                // If Id is not provided, Guid.Empty indicates "new address".
                // OrderService can decide whether to create or ignore based on its rules.
                Id = address.Id ?? Guid.Empty,

                // Attach userId so service can validate ownership and store correctly.
                UserId = userId,

                // Copy address fields
                AddressLine1 = address.AddressLine1,
                AddressLine2 = address.AddressLine2,
                City = address.City,
                State = address.State,
                PostalCode = address.PostalCode,
                Country = address.Country,

                // Flags used to mark default addresses
                IsDefaultBilling = address.IsDefaultBilling,
                IsDefaultShipping = address.IsDefaultShipping
            };
        }
    }
}
