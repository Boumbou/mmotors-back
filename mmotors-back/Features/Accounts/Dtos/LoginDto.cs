using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

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
}