using Grpc.Core;
using UserService.Application.Services;
using ECommerce.GrpcContracts.Users;

namespace UserService.API.GrpcServices
{
    // ============================================================================
    // gRPC Server Implementation for UserService
    // ----------------------------------------------------------------------------
    // This class IMPLEMENTS the RPC operations defined in user.proto:
    //
    // service UserGrpc {
    //   rpc UserExists(...)
    //   rpc GetUserById(...)
    //   rpc GetUserAddressById(...)
    //   rpc SaveOrUpdateAddress(...)
    // }
    //
    // The generated base class is: UserGrpc.UserGrpcBase
    // We extend it and provide the actual implementation using our IUserService.
    // ============================================================================

    public sealed class UserGrpcService : UserGrpc.UserGrpcBase
    {
        private readonly IUserService _userService;   // Application-layer UserService
        private readonly ILogger<UserGrpcService> _logger; // Logger to log messages

        // DI constructor — receives IUserService via dependency injection.
        public UserGrpcService(IUserService userService, ILogger<UserGrpcService> logger)
        {
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _logger = logger;
        }

        // ============================================================================
        // 1. USER EXISTS
        // ----------------------------------------------------------------------------
        // Purpose:
        //   - Quickly check if a user exists.
        //   - Used by OrderService before order creation.
        //
        // Flow:
        //   (a) Validate incoming UserId (string → Guid)
        //   (b) Call application service IUserService.IsUserExistsAsync
        //   (c) Return a boolean reply
        // ============================================================================
        public override async Task<UserExistsReply> UserExists(UserExistsRequest request, ServerCallContext context)
        {
            _logger.LogInformation(
                "gRPC UserExists called. user_id={UserId}",
                request.UserId);

            // Validate user_id format — must be a GUID.
            if (!Guid.TryParse(request.UserId, out var userId))
            {
                _logger.LogWarning(
                    "Invalid user_id received in UserExists. value={UserId}",
                    request.UserId);

                throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid user_id."));
            }

            // Use existing application service to check existence.
            var exists = await _userService.IsUserExistsAsync(userId);

            _logger.LogInformation(
                "UserExists completed. user_id={UserId}, exists={Exists}",
                userId, exists);

            // Return a protobuf reply message.
            return new UserExistsReply { Exists = exists };
        }

        // ============================================================================
        // 2. GET USER BY ID
        // ----------------------------------------------------------------------------
        // Purpose:
        //   - Fetch complete user profile
        //   - Used during order creation for delivery and payment info.
        //
        // Steps:
        //   (a) Validate GUID input
        //   (b) Call _userService.GetProfileAsync
        //   (c) Map application DTO → protobuf User message
        //   (d) Return reply
        // ============================================================================
        public override async Task<GetUserByIdReply> GetUserById(GetUserByIdRequest request, ServerCallContext context)
        {
            _logger.LogInformation(
                "gRPC GetUserById called. user_id={UserId}",
                request.UserId);

            // Validate user_id format.
            if (!Guid.TryParse(request.UserId, out var userId))
            {
                _logger.LogWarning(
                    "Invalid user_id received in GetUserById. value={UserId}",
                    request.UserId);

                throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid user_id."));
            }

            // Get user profile from existing application logic.
            var profile = await _userService.GetProfileAsync(userId);

            // If not found → return Found = false
            if (profile == null)
            {
                _logger.LogInformation(
                    "User not found. user_id={UserId}",
                    userId);

                return new GetUserByIdReply { Found = false };
            }

            _logger.LogInformation(
                "User profile retrieved successfully. user_id={UserId}",
                userId);

            // Map UserService.Application.DTO → Protobuf User message
            return new GetUserByIdReply
            {
                Found = true,
                User = new ECommerce.GrpcContracts.Users.User
                {
                    Id = profile.UserId.ToString(),
                    FullName = profile.FullName ?? string.Empty,
                    Email = profile.Email ?? string.Empty,
                    PhoneNumber = profile.PhoneNumber ?? string.Empty
                }
            };
        }

        // ============================================================================
        // 3. GET USER ADDRESS BY ID
        // ----------------------------------------------------------------------------
        // Purpose:
        //   - Fetch a specific address belonging to a user
        //   - Used during order creation to validate shipping/billing addresses
        //
        // Steps:
        //   (a) Validate both GUIDs (userId + addressId)
        //   (b) Call _userService.GetAddressByUserIdAndAddressIdAsync
        //   (c) Map application-layer AddressDTO → protobuf Address message
        // ============================================================================
        public override async Task<GetUserAddressByIdReply> GetUserAddressById(GetUserAddressByIdRequest request, ServerCallContext context)
        {
            _logger.LogInformation(
               "gRPC GetUserAddressById called. user_id={UserId}, address_id={AddressId}",
               request.UserId, request.AddressId);

            // Validate user ID
            if (!Guid.TryParse(request.UserId, out var userId))
            {
                _logger.LogWarning(
                    "Invalid user_id in GetUserAddressById. value={UserId}",
                    request.UserId);

                throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid user_id."));
            }

            // Validate address ID
            if (!Guid.TryParse(request.AddressId, out var addressId))
            {
                _logger.LogWarning(
                    "Invalid address_id in GetUserAddressById. value={AddressId}",
                    request.AddressId);

                throw new RpcException(
                    new Status(StatusCode.InvalidArgument, "Invalid address_id."));
            }

            // Get address via existing domain/application rules.
            var address = await _userService.GetAddressByUserIdAndAddressIdAsync(userId, addressId);

            // If address does not exist → Found = false
            if (address == null)
            {
                _logger.LogInformation(
                    "Address not found. user_id={UserId}, address_id={AddressId}",
                    userId, addressId);

                return new GetUserAddressByIdReply { Found = false };
            }

            _logger.LogInformation(
               "Address retrieved successfully. user_id={UserId}, address_id={AddressId}",
               userId, addressId);

            // Map Domain AddressDTO → Protobuf Address message.
            return new GetUserAddressByIdReply
            {
                Found = true,
                Address = new ECommerce.GrpcContracts.Users.Address
                {
                    Id = address.Id?.ToString() ?? string.Empty,
                    UserId = address.userId.ToString(),
                    AddressLine1 = address.AddressLine1,
                    AddressLine2 = address.AddressLine2 ?? string.Empty,
                    City = address.City,
                    State = address.State,
                    PostalCode = address.PostalCode,
                    Country = address.Country,
                    IsDefaultBilling = address.IsDefaultBilling,
                    IsDefaultShipping = address.IsDefaultShipping
                }
            };
        }

        // ============================================================================
        // 4. SAVE OR UPDATE ADDRESS
        // ----------------------------------------------------------------------------
        // Purpose:
        //   - Add a new address OR update an existing one.
        //   - Part of user profile management.
        //
        // Steps:
        //   (a) Validate address is not null
        //   (b) Validate userId and (optional) addressId
        //   (c) Convert protobuf Address → AddressDTO (application model)
        //   (d) Use _userService.AddOrUpdateAddressAsync
        //   (e) Return the updated/new addressId
        // ============================================================================
        public override async Task<SaveOrUpdateAddressReply> SaveOrUpdateAddress(SaveOrUpdateAddressRequest request, ServerCallContext context)
        {
            _logger.LogInformation("gRPC SaveOrUpdateAddress called.");

            // Address must be provided.
            if (request.Address == null)
            {
                _logger.LogWarning("SaveOrUpdateAddress failed. Address is null.");

                throw new RpcException(
                    new Status(StatusCode.InvalidArgument, "Address is required."));
            }

            var a = request.Address;

            // Validate and convert UserId
            if (!Guid.TryParse(a.UserId, out var userId))
            {
                _logger.LogWarning(
                    "Invalid user_id in SaveOrUpdateAddress. value={UserId}",
                    a.UserId);

                throw new RpcException(
                    new Status(StatusCode.InvalidArgument, "Invalid user_id."));
            }

            // Optional AddressId (only if updating)
            Guid? addressId = null;

            if (!string.IsNullOrWhiteSpace(a.Id))
            {
                if (!Guid.TryParse(a.Id, out var parsedId))
                {
                    _logger.LogWarning(
                        "Invalid address_id in SaveOrUpdateAddress. value={AddressId}",
                        a.Id);

                    throw new RpcException(
                        new Status(StatusCode.InvalidArgument, "Invalid address_id."));
                }

                addressId = parsedId;
            }

            // Convert protobuf Address → Application DTO (AddressDTO)
            var dto = new UserService.Application.DTOs.AddressDTO
            {
                Id = addressId,
                userId = userId,
                AddressLine1 = a.AddressLine1,
                AddressLine2 = string.IsNullOrWhiteSpace(a.AddressLine2) ? null : a.AddressLine2,
                City = a.City,
                State = a.State,
                PostalCode = a.PostalCode,
                Country = a.Country,
                IsDefaultBilling = a.IsDefaultBilling,
                IsDefaultShipping = a.IsDefaultShipping
            };

            // Save or update using the application-layer method.
            var savedId = await _userService.AddOrUpdateAddressAsync(dto);

            _logger.LogInformation(
                "Address saved successfully. user_id={UserId}, address_id={AddressId}",
                userId, savedId);

            // Return addressId to the caller (OrderService / UI service).
            return new SaveOrUpdateAddressReply { AddressId = savedId.ToString() };
        }
    }
}
