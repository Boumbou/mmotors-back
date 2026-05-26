using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace mmotors_back.Features.Accounts.Dtos
{
    public class LoginDto
    {
        [EmailAddress]
        public required string Email { get; set; }
        [Required]
        [DataType(DataType.Password)]
        public required string Password { get; set; }
    }

    public class LoginResultDto
    {
        public required SignInResult Result { get; set; }
        public UserDto? User { get; set; }
        public string Token { get; set; } = string.Empty;
        public IEnumerable<string> Roles { get; set; } = Enumerable.Empty<string>();
    }
}