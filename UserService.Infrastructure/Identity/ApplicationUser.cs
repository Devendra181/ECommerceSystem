using Microsoft.AspNetCore.Identity;
using UserService.Domain.Entities;

namespace UserService.Infrastructure.Identity
{
    //Here, Guid will be the data type of Primary column in the Roles table
    //You can also specify String, Integer 
    public class ApplicationUser : IdentityUser<Guid>
    {
        public DateTime CreatedAt { get; set; }
        public bool IsActive { get; set; } = true;
        public string? FullName { get; set; }
        public string? ProfilePhotoUrl { get; set; }
        public bool IsEmailConfirmed { get; set; }
        public DateTime? LastLoginAt { get; set; }
        public ICollection<Address> Addresses { get; set; } = new List<Address>();
        public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
    }
}
