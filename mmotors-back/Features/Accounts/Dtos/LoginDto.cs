using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace mmotors_back.Features.Accounts.Dtos
{
    public class LoginDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }

    public class LoginResultDto
    {
        public SignInResult Result { get; set; }
        public UserDto? User { get; set; }
        public string Token { get; set; } = string.Empty;
    }
}