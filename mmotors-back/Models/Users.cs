using Microsoft.AspNetCore.Identity;

namespace mmotors_back.Models

{
    public class User : IdentityUser
    {
        public string? Name { get; set; }
        public string? LastName { get; set; }
        public DateOnly Created { get; set; } = DateOnly.FromDateTime(DateTime.Now);
        public int? Token { get; set; }
        public DateTime? TokenExpiration { get; set; }

    }
}