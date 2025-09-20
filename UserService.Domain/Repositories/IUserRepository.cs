using UserService.Domain.Entities;
namespace UserService.Domain.Repositories
{
    public interface IUserRepository
    {
        Task<User?> FindByEmailAsync(string email);
        Task<User?> FindByUserNameAsync(string userName);
        Task<User?> FindByIdAsync(Guid id);
        Task<bool> CreateUserAsync(User user, string password);
        Task<bool> CheckPasswordAsync(User user, string password);
        Task<bool> UpdateUserAsync(User user);
        Task<IList<string>> GetUserRolesAsync(User user);
        Task<bool> AddUserToRoleAsync(User user, string role);
        Task<bool> VerifyConfirmaionEmailAsync(User user, string token);
        Task<string?> GenerateEmailConfirmationTokenAsync(User user);
        Task<string?> GeneratePasswordResetTokenAsync(User user);
        Task<bool> ResetPasswordAsync(User user, string token, string newPassword);
        Task<bool> ChangePasswordAsync(User user, string currentPassword, string newPassword);
        Task UpdateLastLoginAsync(User user, DateTime loginTime);
        Task<string> GenerateAndStoreRefreshTokenAsync(Guid userId, string clientId, string userAgent, string ipAddress);
        Task<RefreshToken?> GetRefreshTokenAsync(string token);
        Task RevokeRefreshTokenAsync(RefreshToken refreshToken, string ipAddress);
        Task<List<Address>> GetAddressesByUserIdAsync(Guid userId);
        Task<Guid> AddOrUpdateAddressAsync(Address address);
        Task<bool> DeleteAddressAsync(Guid userId, Guid addressId);
        Task<bool> IsLockedOutAsync(User user);
        Task<bool> IsTwoFactorEnabledAsync(User user);
        Task IncrementAccessFailedCountAsync(User user);
        Task ResetAccessFailedCountAsync(User user);
        Task<DateTime?> GetLockoutEndDateAsync(User user);
        Task<int> GetMaxFailedAccessAttemptsAsync();
        Task<int> GetAccessFailedCountAsync(User user);
        Task<bool> IsValidClientAsync(string clientId);
        Task<bool> IsUserExistsAsync(Guid userId);
        Task<Address?> GetAddressByUserIdAndAddressIdAsync(Guid userId, Guid addressId);
    }
}
