
namespace mmotors_back.Features.Accounts.Dtos
{
	public class UserDto
	{
		public string Id { get; set; }
		public DateOnly Created { get; set; }
		public string Email { get; set; }
		public string Name { get; set; }
		public string LastName { get; set; }
		public string AuthToken { get; set; }
	}

	public class VerifyTokenDto
	{
		public string Email { get; set; }
		public int Token { get; set; }
	}
}