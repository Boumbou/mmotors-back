//implememtation of UserMapper class
using System.Data.Common;
using mmotors_back.Features.Accounts.Dtos;
using mmotors_back.Models;
namespace mmotors_back.Mappers
{   
    public class UserMapper
    {
        public static UserDto ToDTO(User user)
        {
            return new  UserDto
            {
                Id = user.Id,
                Created = user.Created,
                Email = user.Email,
                Name = user.Name,
                LastName = user.LastName,
                AuthToken = user.Token.HasValue ? user.Token.Value.ToString() : null
            };
        }

        public static User ToEntity(UserDto userDTO)
        {
            return new User
            {
                Id = userDTO.Id,
                Created = userDTO.Created,
                Email = userDTO.Email,
                Name = userDTO.Name,
                LastName = userDTO.LastName
            };
        }

        public static User RegisterDtoToEntity(RegisterDto registerDto)
        {
            return new User
            {
                Email = registerDto.Email,
                UserName = registerDto.Email,
                Name = registerDto.Name,
                LastName = registerDto.LastName
            };
        }

        public static User LoginDtoToEntity(LoginDto loginDto)
        {
            return new User
            {
                Email = loginDto.Email
            };
        }
    }
}