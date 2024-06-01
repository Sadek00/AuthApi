using Microsoft.AspNetCore.Identity;

namespace AuthApi.Models
{
    public class AppUser : IdentityUser
    {
        public string? FirstName { get; set; }
        public required string LastName { get; set; }
    }
}
