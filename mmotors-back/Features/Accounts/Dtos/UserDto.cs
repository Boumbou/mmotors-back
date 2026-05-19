
namespace mmotors_back.Features.Accounts.Dtos
{
	public class UserDto
	{
		public required string Id { get; set; }
		public required DateOnly Created { get; set; }
		public required string Email { get; set; }
		public required string Name { get; set; }
		public required string LastName { get; set; }
	}

	public class VerifyTokenDto
	{
		public required string Email { get; set; }
		public int Token { get; set; }
	}
}