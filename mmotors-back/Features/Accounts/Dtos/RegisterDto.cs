using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace mmotors_back.Features.Accounts.Dtos
{
    public class RegisterDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public string LastName { get; set; }
        [Required]
        public string Password { get; set; }
    }

    public class RegisterResultDto
    {
        public IdentityResult Result { get; set; }
        public UserDto? User { get; set; }
        public string Token { get; set; } = string.Empty;
    }
}

