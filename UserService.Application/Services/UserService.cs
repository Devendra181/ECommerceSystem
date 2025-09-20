using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using UserService.Application.DTOs;
using UserService.Domain.Entities;
using UserService.Domain.Repositories;

namespace UserService.Application.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IConfiguration _configuration;

        public UserService(IUserRepository userRepository, IConfiguration configuration)
        {
            _userRepository = userRepository;
            _configuration = configuration;
        }

        public async Task<bool> RegisterAsync(RegisterDTO dto)
        {
            if (await _userRepository.FindByEmailAsync(dto.Email) != null) 
                return false;

            if (await _userRepository.FindByUserNameAsync(dto.UserName) != null) 
                return false;

            var user = new User
            {
                Id = Guid.NewGuid(),
                UserName = dto.UserName,
                Email = dto.Email,
                PhoneNumber = dto.PhoneNumber,
                FullName = dto.FullName,
                CreatedAt = DateTime.UtcNow,
                IsActive = true,
                IsEmailConfirmed = false
            };

            var created = await _userRepository.CreateUserAsync(user, dto.Password);
            if (!created) 
                return false;

            await _userRepository.AddUserToRoleAsync(user, "Customer");

            return true;
        }

        public async Task<LoginResponseDTO> LoginAsync(LoginDTO dto, string ipAddress, string userAgent)
        {
            var response = new LoginResponseDTO();

            // Validate Client
            if (!await _userRepository.IsValidClientAsync(dto.ClientId))
            {
                response.ErrorMessage = "Invalid client ID.";
                return response;
            }

            // Get user by email or username
            var user = dto.EmailOrUserName.Contains("@")
                ? await _userRepository.FindByEmailAsync(dto.EmailOrUserName)
                : await _userRepository.FindByUserNameAsync(dto.EmailOrUserName);

            if (user == null)
            {
                response.ErrorMessage = "Invalid username or password.";
                return response;
            }

            // Check lockout info
            if (await _userRepository.IsLockedOutAsync(user))
            {
                var lockoutEnd = await _userRepository.GetLockoutEndDateAsync(user);
                if (lockoutEnd.HasValue && lockoutEnd > DateTime.UtcNow)
                {
                    var timeLeft = lockoutEnd.Value - DateTime.UtcNow;
                    response.ErrorMessage = $"Account is locked. Try again after {timeLeft.Minutes} minute(s) and {timeLeft.Seconds} second(s).";
                    response.RemainingAttempts = 0;
                    return response;
                }
                else
                {
                    await _userRepository.ResetAccessFailedCountAsync(user);
                }
            }

            if (!user.IsEmailConfirmed)
            {
                response.ErrorMessage = "Email not confirmed. Please verify your email.";
                return response;
            }

            // Validate password
            var passwordValid = await _userRepository.CheckPasswordAsync(user, dto.Password);
            if (!passwordValid)
            {
                await _userRepository.IncrementAccessFailedCountAsync(user);

                if (await _userRepository.IsLockedOutAsync(user))
                {
                    response.ErrorMessage = "Account locked due to multiple failed login attempts.";
                    response.RemainingAttempts = 0;
                    return response;
                }

                var maxAttempts = await _userRepository.GetMaxFailedAccessAttemptsAsync();
                var failedCount = await _userRepository.GetAccessFailedCountAsync(user);
                var attemptsLeft = maxAttempts - failedCount;

                response.ErrorMessage = "Invalid username or password.";
                response.RemainingAttempts = attemptsLeft > 0 ? attemptsLeft : 0;
                return response;
            }

            await _userRepository.ResetAccessFailedCountAsync(user);

            if (await _userRepository.IsTwoFactorEnabledAsync(user))
            {
                response.RequiresTwoFactor = true;
                return response;
            }

            await _userRepository.UpdateLastLoginAsync(user, DateTime.UtcNow);

            var roles = await _userRepository.GetUserRolesAsync(user);

            response.Token = GenerateJwtToken(user, roles, dto.ClientId);
            response.RefreshToken = await _userRepository.GenerateAndStoreRefreshTokenAsync(user.Id,  dto.ClientId, userAgent, ipAddress);

            return response;
        }

        public async Task<EmailConfirmationTokenResponseDTO?> SendConfirmationEmailAsync(string email)
        {
            EmailConfirmationTokenResponseDTO? emailConfirmationTokenResponseDTO = null;
            var user = await _userRepository.FindByEmailAsync(email);
            if (user == null)
                return null;

            var token = await _userRepository.GenerateEmailConfirmationTokenAsync(user);

            if (token != null)
            {
                emailConfirmationTokenResponseDTO = new EmailConfirmationTokenResponseDTO()
                {
                    UserId = user.Id,
                    Token = token
                };
            }

            return emailConfirmationTokenResponseDTO;
        }

        public async Task<bool> VerifyConfirmationEmailAsync(ConfirmEmailDTO dto)
        {
            var user = await _userRepository.FindByIdAsync(dto.UserId);
            if (user == null)
                return false;

            var result = await _userRepository.VerifyConfirmaionEmailAsync(user, dto.Token);
            if (result)
            {
                user.IsActive = true;
                await _userRepository.UpdateUserAsync(user);
            }
            return result;
        }

        public async Task<RefreshTokenResponseDTO> RefreshTokenAsync(RefreshTokenRequestDTO dto, string ipAddress, string userAgent)
        {
            var response = new RefreshTokenResponseDTO();

            // Validate Client
            if (!await _userRepository.IsValidClientAsync(dto.ClientId))
            {
                response.ErrorMessage = "Invalid client ID.";
                return response;
            }

            var refreshTokenEntity = await _userRepository.GetRefreshTokenAsync(dto.RefreshToken);

            if (refreshTokenEntity == null || !refreshTokenEntity.IsActive)
            {
                response.ErrorMessage = "Invalid or expired refresh token.";
                return response;
            }

            // Revoke the old refresh token and generate a new one
            var newRefreshToken = await _userRepository.GenerateAndStoreRefreshTokenAsync(refreshTokenEntity.UserId, dto.ClientId, userAgent, ipAddress);

            var user = await _userRepository.FindByIdAsync(refreshTokenEntity.UserId);
            if (user == null)
            {
                response.ErrorMessage = "User not found.";
                return response;
            }

            var roles = await _userRepository.GetUserRolesAsync(user);

            response.Token = GenerateJwtToken(user, roles, dto.ClientId);
            response.RefreshToken = newRefreshToken;

            return response;
        }

        public async Task<bool> RevokeRefreshTokenAsync(string token, string ipAddress)
        {
            var refreshToken = await _userRepository.GetRefreshTokenAsync(token);
            if (refreshToken == null || !refreshToken.IsActive)
                return false;

            await _userRepository.RevokeRefreshTokenAsync(refreshToken, ipAddress);
            return true;
        }

        public async Task<ForgotPasswordResponseDTO?> ForgotPasswordAsync(string email)
        {
            ForgotPasswordResponseDTO? forgotPasswordResponseDTO = null;

            var user = await _userRepository.FindByEmailAsync(email);
            if (user == null) 
                return null;

            var token = await _userRepository.GeneratePasswordResetTokenAsync(user);

            if (token != null)
            {
                forgotPasswordResponseDTO = new ForgotPasswordResponseDTO()
                {
                    UserId = user.Id,
                    Token = token
                };
            }

            return forgotPasswordResponseDTO;
        }

        public async Task<bool> ChangePasswordAsync(Guid userId, string currentPassword, string newPassword)
        {
            var user = await _userRepository.FindByIdAsync(userId);
            if (user == null) 
                return false;

            return await _userRepository.ChangePasswordAsync(user, currentPassword, newPassword);
        }

        public async Task<bool> ResetPasswordAsync(Guid userId, string token, string newPassword)
        {
            var user = await _userRepository.FindByIdAsync(userId);
            if (user == null) return false;

            return await _userRepository.ResetPasswordAsync(user, token, newPassword);
        }

        public async Task<ProfileDTO?> GetProfileAsync(Guid userId)
        {
            var user = await _userRepository.FindByIdAsync(userId);
            if (user == null) return null;

            return new ProfileDTO
            {
                UserId = user.Id,
                FullName = user.FullName,
                PhoneNumber = user.PhoneNumber,
                ProfilePhotoUrl = user.ProfilePhotoUrl,
                Email = user.Email,
                LastLoginAt = user.LastLoginAt,
                UserName = user.UserName
            };
        }

        public async Task<bool> UpdateProfileAsync(UpdateProfileDTO dto)
        {
            var user = await _userRepository.FindByIdAsync(dto.UserId);
            if (user == null) 
                return false;

            user.FullName = dto.FullName;
            user.PhoneNumber = dto.PhoneNumber;
            user.ProfilePhotoUrl = dto.ProfilePhotoUrl;

            return await _userRepository.UpdateUserAsync(user);
        }

        public async Task<IEnumerable<AddressDTO>> GetAddressesAsync(Guid userId)
        {
            var addresses = await _userRepository.GetAddressesByUserIdAsync(userId);
            return addresses.Select(a => new AddressDTO
            {
                Id = a.Id,
                AddressLine1 = a.AddressLine1,
                AddressLine2 = a.AddressLine2,
                City = a.City,
                State = a.State,
                PostalCode = a.PostalCode,
                Country = a.Country,
                IsDefaultBilling = a.IsDefaultBilling,
                IsDefaultShipping = a.IsDefaultShipping
            });
        }

        public async Task<Guid> AddOrUpdateAddressAsync(AddressDTO dto)
        {
            var address = new Address
            {
                Id = dto.Id ?? Guid.NewGuid(),
                UserId = dto.userId,
                AddressLine1 = dto.AddressLine1,
                AddressLine2 = dto.AddressLine2,
                City = dto.City,
                State = dto.State,
                PostalCode = dto.PostalCode,
                Country = dto.Country,
                IsDefaultBilling = dto.IsDefaultBilling,
                IsDefaultShipping = dto.IsDefaultShipping
            };

            var addressId = await _userRepository.AddOrUpdateAddressAsync(address);
            return addressId;
        }

        public async Task<bool> DeleteAddressAsync(Guid userId, Guid addressId)
        {
            return await _userRepository.DeleteAddressAsync(userId, addressId);
        }

        public async Task<bool> IsUserExistsAsync(Guid userId)
        {
            return await _userRepository.IsUserExistsAsync(userId);
        }

        public async Task<AddressDTO?> GetAddressByUserIdAndAddressIdAsync(Guid userId, Guid addressId)
        {
            var address = await _userRepository.GetAddressByUserIdAndAddressIdAsync(userId, addressId);
            if(address != null)
            {
                return new AddressDTO
                {
                    Id = address.Id,
                    AddressLine1 = address.AddressLine1,
                    AddressLine2 = address.AddressLine2,
                    City = address.City,
                    State = address.State,
                    PostalCode = address.PostalCode,
                    Country = address.Country,
                    IsDefaultBilling = address.IsDefaultBilling,
                    IsDefaultShipping = address.IsDefaultShipping
                };
            }

            return null;
        }

        private string GenerateJwtToken(User user, IList<string> roles, string clientId)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email ?? ""),
                new Claim(ClaimTypes.Name, user.UserName ?? ""),
                new Claim("client_id", clientId)
            };

            // Add role claims
            claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

            // Read JWT settings from configuration
            var secretKey = _configuration["JwtSettings:SecretKey"];
            var issuer = _configuration["JwtSettings:Issuer"];
            var expiryMinutes = Convert.ToInt32(_configuration["JwtSettings:AccessTokenExpirationMinutes"]);

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var tokenDescriptor = new JwtSecurityToken(
                issuer: issuer,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(expiryMinutes),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(tokenDescriptor);
        }
    }
}
