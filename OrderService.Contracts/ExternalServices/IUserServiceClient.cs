using OrderService.Contracts.DTOs;
namespace OrderService.Contracts.ExternalServices
{
    public interface IUserServiceClient
    {
        Task<bool> UserExistsAsync(Guid userId, string accessToken);
        Task<UserDTO?> GetUserByIdAsync(Guid userId, string accessToken);
        Task<AddressDTO?> GetUserAddressByIdAsync(Guid userId, Guid addressId, string accessToken);
        Task<Guid?> SaveOrUpdateAddressAsync(AddressDTO addressDto, string accessToken);
    }
}
