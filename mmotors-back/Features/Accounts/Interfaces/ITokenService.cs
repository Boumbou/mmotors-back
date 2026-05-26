
using mmotors_back.Models;

namespace mmotors_back.Features.Accounts.Interfaces
{

    public interface ITokenService
    {
        string GenerateToken(User user, IEnumerable<string> roles);
    }
}