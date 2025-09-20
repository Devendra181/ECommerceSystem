using Microsoft.AspNetCore.Identity;
namespace UserService.Infrastructure.Identity
{
    //Here, Guid will be the data type of Primary column in the Roles table
    //You can also specify String, Integer 
    public class ApplicationRole : IdentityRole<Guid>
    {
        public string? Description { get; set; }
    }
}
