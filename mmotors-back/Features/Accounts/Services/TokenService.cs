using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

using mmotors_back.Features.Accounts.Interfaces;
using mmotors_back.Models;

namespace mmotors_back.Features.Accounts.Services
{
    public class TokenService : ITokenService
    {
        // This class will contain methods for:
        // - Generating JWT tokens for authenticated users
        // - Validating and refreshing tokens
        // - Managing token expiration and revocation
        private readonly IConfiguration _config; // holds configuration properties
        private readonly SymmetricSecurityKey _key;

        public TokenService(IConfiguration config)
        {
            _config = config;
            _key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["JWT:SigningKey"]!));
        }

        public string GenerateToken(User user, IEnumerable<string> roles)
        {

            //method to invoke to generate token
            
            //null check
            if (user == null) throw new ArgumentNullException("user");

            //get user roles
            //create claims
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Id.ToString()),

                //add user id claim
                new Claim(JwtRegisteredClaimNames.NameId, user.Id),
                //add user created at claim
                new Claim(JwtRegisteredClaimNames.Iat, user.Created.ToString()),
                //add user name claim
                new Claim(JwtRegisteredClaimNames.Name, user.Name!),
                //add user email claim
                new Claim(JwtRegisteredClaimNames.Email, user.Email!),
                //add user name claim
                new Claim(JwtRegisteredClaimNames.GivenName, user.Email!),

            };
            //add roles to claims
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }
            //create signing credentials
            var creds = new SigningCredentials(_key, SecurityAlgorithms.HmacSha512Signature);
            //create token descriptor
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.Now.AddDays(7),
                SigningCredentials = creds,
                Issuer = _config["JWT:Issuer"],
                Audience = _config["JWT:Audience"]
            };

            var tokenHandler = new JwtSecurityTokenHandler();

            var token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
        }
    }
}