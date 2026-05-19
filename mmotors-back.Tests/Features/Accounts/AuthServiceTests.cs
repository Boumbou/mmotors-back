using FluentAssertions;
using mmotors_back.Models;
using mmotors_back.Features.Accounts.Dtos;
using mmotors_back.Features.Accounts.Services;
using mmotors_back.Features.Accounts.Interfaces;
using Microsoft.AspNetCore.Identity;
using Moq;

namespace mmotors_back.Tests.Features.Accounts;

public class AuthServiceTests
{

    //centralise mocking of UserManager and SignInManager to avoid repetition
    private Mock<UserManager<User>> GetUserManagerMock()
    {
        return new Mock<UserManager<User>>(
            Mock.Of<IUserStore<User>>(), null!, null!, null!, null!, null!, null!, null!, null!);
    }   
    private Mock<SignInManager<User>> GetSignInManagerMock(Mock<UserManager<User>> userManagerMock)
    {
        return new Mock<SignInManager<User>>(
            userManagerMock.Object,
            Mock.Of<Microsoft.AspNetCore.Http.IHttpContextAccessor>(),
            Mock.Of<IUserClaimsPrincipalFactory<User>>(),
            null!, null!, null!, null!);
    }

    private Mock<ITokenService> GetTokenServiceMock()
    {
        return new Mock<ITokenService>();
    }


    //verify registration logic with valid data
    [Fact]
    public async Task Register_ShouldSucceed_WhenDataIsValid()
    {
        // Arrange
        var userManagerMock = GetUserManagerMock();

        userManagerMock.Setup(um => um.CreateAsync(It.IsAny<User>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success);

        var tokenServiceMock = GetTokenServiceMock();
        tokenServiceMock.Setup(ts => ts.GenerateToken(It.IsAny<User>())).Returns("mocked_token");

        var signInManagerMock = GetSignInManagerMock(userManagerMock);
        signInManagerMock.Setup(sm => sm.PasswordSignInAsync(It.IsAny<User>(), It.IsAny<string>(), false, false))
            .ReturnsAsync(SignInResult.Success);
        
        
        var authService = new AuthService(userManagerMock.Object, signInManagerMock.Object, null!, tokenServiceMock.Object);
        var registerDto = new RegisterDto
        {
            Name = "Test User",
            LastName = "User",
            Email = "test@example.com",
            Password = "Password123!"
        };
    
        // Act
        var result = await authService.RegisterUserAsync(registerDto);
    
        // Assert
        result.Result.Succeeded.Should().BeTrue();
        result.Token.Should().NotBeNullOrEmpty();
        userManagerMock.Verify(um => um.CreateAsync(It.Is<User>(u => u.Email == registerDto.Email), registerDto.Password), Times.Once); 
    }

    //test register success should return UserDto and token
    [Fact]
    public async Task Register_ShouldReturnUserDtoAndToken_WhenRegistrationIsSuccessful()
    {
        // Arrange
        var userManagerMock = GetUserManagerMock();

        userManagerMock.Setup(um => um.CreateAsync(It.IsAny<User>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success);

        var tokenServiceMock = GetTokenServiceMock();
        tokenServiceMock.Setup(ts => ts.GenerateToken(It.IsAny<User>())).Returns("mocked_token");

        var signInManagerMock = GetSignInManagerMock(userManagerMock);
        signInManagerMock.Setup(sm => sm.PasswordSignInAsync(It.IsAny<User>(), It.IsAny<string>(), false, false))
            .ReturnsAsync(SignInResult.Success);
        
        var authService = new AuthService(userManagerMock.Object, signInManagerMock.Object, null!, tokenServiceMock.Object);
        var registerDto = new RegisterDto
        {
            Name = "Test User",
            LastName = "User",
            Email = "test@example.com",
            Password = "Password123!"
        };
        // Act
        var result = await authService.RegisterUserAsync(registerDto);
        // Assert
        result.Result.Succeeded.Should().BeTrue();
        result.Token.Should().NotBeNullOrEmpty();
        result.User.Should().NotBeNull();
        result.User.Email.Should().Be(registerDto.Email);
    }

    // verify registration fails with duplicate emails
    [Fact]
    public async Task Register_ShouldFail_WhenEmailAlreadyExists()
    {
        // Arrange
        var userManagerMock = GetUserManagerMock();

        userManagerMock.Setup(um => um.FindByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync(new User { Email = "test@example.com" });

        userManagerMock.Setup(um => um.CreateAsync(It.IsAny<User>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Email already exists" }));

        var authService = new AuthService(userManagerMock.Object, GetSignInManagerMock(userManagerMock).Object, null!, GetTokenServiceMock().Object);
        var registerDto = new RegisterDto
        {
            Name = "Test User",
            LastName = "User",
            Email = "test@example.com",
            Password = "Password123!"
        };

        // Act
        var resultDuplicate = await authService.RegisterUserAsync(registerDto);

        // Assert
        resultDuplicate.Result.Succeeded.Should().BeFalse();
        resultDuplicate.Result.Errors.Should().ContainSingle(e => e.Description == "Email already exists");
        resultDuplicate.Token.Should().BeNullOrEmpty();

        userManagerMock.Verify(um => um.CreateAsync(It.Is<User>(u => u.Email == registerDto.Email), registerDto.Password), Times.Never);
        userManagerMock.Verify(um => um.FindByEmailAsync(It.Is<string>(email => email == registerDto.Email)), Times.Once);
    }

    //verify registration fails with empty email
    [Fact]
    public async Task Register_ShouldFail_WhenEmailIsEmpty()
    {
        // Arrange
        var userManagerMock = GetUserManagerMock();
        var authService = new AuthService(userManagerMock.Object, GetSignInManagerMock(userManagerMock).Object, null!, GetTokenServiceMock().Object);
        var registerDto = new RegisterDto
        {
            Name = "Test User",
            LastName = "User",
            Email = "",
            Password = "Password123!"
        };
        // Act
        var result = await authService.RegisterUserAsync(registerDto);

        // Assert
        result.Result.Succeeded.Should().BeFalse();
        result.Result.Errors.Should().ContainSingle(e => e.Description == "Email is required");
        result.Token.Should().BeNullOrEmpty();
        userManagerMock.Verify(um => um.CreateAsync(It.IsAny<User>(), It.IsAny<string>()), Times.Never);
    }

    //verify registration fails with empty password
    [Fact]    public async Task Register_ShouldFail_WhenPasswordIsEmpty()
    {
        // Arrange
        var userManagerMock = GetUserManagerMock();
        var authService = new AuthService(userManagerMock.Object, GetSignInManagerMock(userManagerMock).Object, null!, GetTokenServiceMock().Object);
        var registerDto = new RegisterDto
        {
            Name = "Test User",
            LastName = "User",
            Email = "test@example.com",
            Password = ""
        };  
        // Act
        var result = await authService.RegisterUserAsync(registerDto);
        // Assert
        result.Result.Succeeded.Should().BeFalse();
        result.Result.Errors.Should().ContainSingle(e => e.Description == "Password is required");
        result.Token.Should().BeNullOrEmpty();
        userManagerMock.Verify(um => um.CreateAsync(It.IsAny<User>(), It.IsAny<string>()), Times.Never);
    }

    //test login logic with valid credentials
    [Fact]
    public async Task Login_ShouldSucceed_WhenCredentialsAreValid()
    {
        // Arrange
        var userManagerMock = GetUserManagerMock();

        userManagerMock.Setup(um => um.FindByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync(new User { Email = "test@example.com" });

        var signInManagerMock = GetSignInManagerMock(userManagerMock);

        signInManagerMock.Setup(sm => sm.PasswordSignInAsync(It.IsAny<User>(), It.IsAny<string>(), false, false))
            .ReturnsAsync(SignInResult.Success);

        var tokenServiceMock = new Mock<ITokenService>();
        tokenServiceMock.Setup(ts => ts.GenerateToken(It.IsAny<User>())).Returns("mocked_token");

        var authService = new AuthService(userManagerMock.Object, signInManagerMock.Object, null!, tokenServiceMock.Object);
        var loginDto = new LoginDto
        {
            Email = "test@example.com",
            Password = "Password123!"
        };

        // Act
        LoginResultDto result = await authService.LoginUserAsync(loginDto);

        // Assert
        result.Result.Succeeded.Should().BeTrue();
        result.Token.Should().NotBeNullOrEmpty();
        result.User.Should().NotBeNull();
        result.User.Email.Should().Be(loginDto.Email);
        signInManagerMock.Verify(sm => sm.PasswordSignInAsync(It.Is<User>(u => u.Email == loginDto.Email), loginDto.Password, false, false), Times.Once);
    }

    //test login fails with invalid credentials
    [Fact]    public async Task Login_ShouldFail_WhenCredentialsAreInvalid()
    {
        // Arrange
        var userManagerMock = GetUserManagerMock();
        userManagerMock.Setup(um => um.FindByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync(new User { Email = "test@example.com" });
        var signInManagerMock = GetSignInManagerMock(userManagerMock);
        signInManagerMock.Setup(sm => sm.PasswordSignInAsync(It.IsAny<User>(), It.IsAny<string>(), false, false))
            .ReturnsAsync(SignInResult.Failed);
        var tokenServiceMock = new Mock<ITokenService>();
        var authService = new AuthService(userManagerMock.Object, signInManagerMock.Object, null!, tokenServiceMock.Object);
        var loginDto = new LoginDto
        {
            Email = "test@example.com",
            Password = "InvalidPassword"
        };
        // Act
        LoginResultDto result = await authService.LoginUserAsync(loginDto);
        // Assert
        result.Result.Succeeded.Should().BeFalse();
        signInManagerMock.Verify(sm => sm.PasswordSignInAsync(It.Is<User>(u => u.Email == loginDto.Email), loginDto.Password, false, false), Times.Once);
    }

    //test login fails when user does not exist
    [Fact]
    public async Task Login_ShouldFail_WhenUserDoesNotExist()
    {
        // Arrange
        var userManagerMock = GetUserManagerMock();
        userManagerMock.Setup(um => um.FindByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync((User)null!);

        var signInManagerMock = GetSignInManagerMock(userManagerMock);

        var authService = new AuthService(userManagerMock.Object, signInManagerMock.Object, null!, GetTokenServiceMock().Object);
        var loginDto = new LoginDto
        {
            Email = "nonexistent@example.com",
            Password = "Password123!"
        };

        // Act
        LoginResultDto result = await authService.LoginUserAsync(loginDto);

        // Assert
        result.Result.Succeeded.Should().BeFalse();
        signInManagerMock.Verify(sm => sm.PasswordSignInAsync(It.IsAny<User>(), loginDto.Password, false, false), Times.Never);
    }

    //test successful login should return token
    [Fact]
    public async Task Login_ShouldNotReturnToken_WhenCredentialsAreInvalid()
    {
        // Arrange
        var userManagerMock = GetUserManagerMock();
        
        userManagerMock.Setup(um => um.FindByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync(new User { Email = "test@example.com" });

        var signInManagerMock = GetSignInManagerMock(userManagerMock);

        signInManagerMock.Setup(sm => sm.PasswordSignInAsync(It.IsAny<User>(), It.IsAny<string>(), false, false))
            .ReturnsAsync(SignInResult.Failed);

        var tokenServiceMock = GetTokenServiceMock();

        var authService = new AuthService(userManagerMock.Object, signInManagerMock.Object, null!, tokenServiceMock.Object);
        var loginDto = new LoginDto
        {
            Email = "test@example.com",
            Password = "ValidPassword123!"
        };
        // Act
        LoginResultDto result = await authService.LoginUserAsync(loginDto);
        // Assert
        // verify token existence here (if implemented)
        //create the assertion for token existence based on your implementation, for example:
        //result.Token.Should().NotBeNullOrEmpty();
        result.Token.Should().BeNullOrEmpty();
        tokenServiceMock.Verify(ts => ts.GenerateToken(It.IsAny<User>()), Times.Never);
    }
}