using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

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
}