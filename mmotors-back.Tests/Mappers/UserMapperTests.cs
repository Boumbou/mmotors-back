using mmotors_back.Features.Accounts.Dtos;
using mmotors_back.Mappers;
using mmotors_back.Models;

namespace mmotors_back.Tests.Mappers;

public class UserMapperTests
{
    #region ToDTO
    [Fact]
    public void ToDTO_ShouldMapAllFields_WhenEntityIsValid()
    {
        // Arrange
        var created = new DateOnly(2026, 6, 3);
        var user = new User
        {
            Id = Guid.NewGuid().ToString(),
            Email = "customer@example.com",
            UserName = "customer@example.com",
            Name = "Ada",
            LastName = "Lovelace",
            Created = created
        };

        // Act
        var result = UserMapper.ToDTO(user);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(user.Id, result.Id);
        Assert.Equal(user.Email, result.Email);
        Assert.Equal(user.Name, result.Name);
        Assert.Equal(user.LastName, result.LastName);
        Assert.Equal(user.Created, result.Created);
    }

    [Fact]
    public void ToDTO_ShouldReturnNull_WhenUserIsNull()
    {
        // Arrange
        User? user = null;

        // Act
        var result = UserMapper.ToDTO(user!);

        // Assert
        Assert.Null(result);
    }
    #endregion

    #region ToEntity
    [Fact]
    public void ToEntity_ShouldMapAllFields_WhenDtoIsValid()
    {
        // Arrange
        var userDto = new UserDto
        {
            Id = Guid.NewGuid().ToString(),
            Email = "customer@example.com",
            Name = "Grace",
            LastName = "Hopper",
            Created = new DateOnly(2026, 1, 14)
        };

        // Act
        var result = UserMapper.ToEntity(userDto);

        // Assert
        Assert.Equal(userDto.Id, result.Id);
        Assert.Equal(userDto.Email, result.Email);
        Assert.Equal(userDto.Name, result.Name);
        Assert.Equal(userDto.LastName, result.LastName);
        Assert.Equal(userDto.Created, result.Created);
    }
    #endregion

    #region RegisterDtoToEntity
    [Fact]
    public void RegisterDtoToEntity_ShouldMapFieldsAndCopyEmailToUserName_WhenDtoIsValid()
    {
        // Arrange
        var registerDto = new RegisterDto
        {
            Email = "register@example.com",
            Name = "Katherine",
            LastName = "Johnson",
            Password = "Password123!"
        };

        // Act
        var result = UserMapper.RegisterDtoToEntity(registerDto);

        // Assert
        Assert.Equal(registerDto.Email, result.Email);
        Assert.Equal(registerDto.Email, result.UserName);
        Assert.Equal(registerDto.Name, result.Name);
        Assert.Equal(registerDto.LastName, result.LastName);
    }
    #endregion

    #region LoginDtoToEntity
    [Fact]
    public void LoginDtoToEntity_ShouldMapEmail_WhenDtoIsValid()
    {
        // Arrange
        var loginDto = new LoginDto
        {
            Email = "login@example.com",
            Password = "Password123!"
        };

        // Act
        var result = UserMapper.LoginDtoToEntity(loginDto);

        // Assert
        Assert.Equal(loginDto.Email, result.Email);
    }
    #endregion
}