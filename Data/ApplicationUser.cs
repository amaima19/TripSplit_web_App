using Microsoft.AspNetCore.Identity;

namespace TripSplit.Data
{
    public class ApplicationUser:IdentityUser
    {
        public string FullName { get; set; } = string.Empty;
    }
}
