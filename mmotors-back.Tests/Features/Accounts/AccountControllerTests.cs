using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using mmotors_back.Features.Accounts.Controllers;
using mmotors_back.Features.Accounts.Dtos;
using mmotors_back.Features.Accounts.Interfaces;
using mmotors_back.Mappers;
using mmotors_back.Models;
using Moq;

namespace mmotors_back.Tests.Features.Accounts;

public class AccountControllerTests
{
    #region Register
    [Fact]
    public async Task Register_ShouldReturnOk_WhenRegistrationSucceeds()
    {
        // Arrange
        var userManagerMock = GetUserManagerMock();
        userManagerMock.Setup(manager => manager.FindByEmailAsync("test@example.com")).ReturnsAsync((User?)null);
        userManagerMock.Setup(manager => manager.CreateAsync(It.IsAny<User>(), "Password123!")).ReturnsAsync(IdentityResult.Success);
        userManagerMock.Setup(manager => manager.AddToRoleAsync(It.IsAny<User>(), "Customer")).ReturnsAsync(IdentityResult.Success);
        var tokenServiceMock = new Mock<ITokenService>();
        tokenServiceMock.Setup(service => service.GenerateToken(It.IsAny<User>(), It.IsAny<IEnumerable<string>>())).Returns("token");
        var controller = CreateController(userManagerMock.Object, tokenServiceMock.Object, true);

        // Act
        var result = await controller.Register(new RegisterDto
        {
            Email = "test@example.com",
            Name = "Test",
            LastName = "User",
            Password = "Password123!"
        });

        // Assert
        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task Register_ShouldReturnBadRequest_WhenModelStateIsInvalid()
    {
        // Arrange
        var controller = CreateController(GetUserManagerMock().Object, new Mock<ITokenService>().Object, true);
        controller.ModelState.AddModelError("Email", "required");

        // Act
        var result = await controller.Register(new RegisterDto
        {
            Email = "test@example.com",
            Name = "Test",
            LastName = "User",
            Password = "Password123!"
        });

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task Register_ShouldReturnBadRequest_WhenRegistrationFails()
    {
        // Arrange
        var userManagerMock = GetUserManagerMock();
        userManagerMock.Setup(manager => manager.FindByEmailAsync("test@example.com")).ReturnsAsync(new User { Id = Guid.NewGuid().ToString(), Email = "test@example.com", UserName = "test@example.com" });
        var controller = CreateController(userManagerMock.Object, new Mock<ITokenService>().Object, true);

        // Act
        var result = await controller.Register(new RegisterDto
        {
            Email = "test@example.com",
            Name = "Test",
            LastName = "User",
            Password = "Password123!"
        });

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }
    #endregion

    #region Login
    [Fact]
    public async Task Login_ShouldReturnOk_WhenCredentialsAreValid()
    {
        // Arrange
        var userManagerMock = GetUserManagerMock();
        userManagerMock.Setup(manager => manager.FindByEmailAsync("test@example.com")).ReturnsAsync(new User { Id = Guid.NewGuid().ToString(), Email = "test@example.com", UserName = "test@example.com", Name = "Test", LastName = "User" });
        userManagerMock.Setup(manager => manager.CheckPasswordAsync(It.IsAny<User>(), "Password123!")).ReturnsAsync(true);
        userManagerMock.Setup(manager => manager.GetRolesAsync(It.IsAny<User>())).ReturnsAsync(new List<string> { "Customer" });
        var tokenServiceMock = new Mock<ITokenService>();
        tokenServiceMock.Setup(service => service.GenerateToken(It.IsAny<User>(), It.IsAny<IEnumerable<string>>())).Returns("token");
        var controller = CreateController(userManagerMock.Object, tokenServiceMock.Object, true);

        // Act
        var result = await controller.Login(new LoginDto
        {
            Email = "test@example.com",
            Password = "Password123!"
        });

        // Assert
        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task Login_ShouldReturnBadRequest_WhenModelStateIsInvalid()
    {
        // Arrange
        var controller = CreateController(GetUserManagerMock().Object, new Mock<ITokenService>().Object, true);
        controller.ModelState.AddModelError("Password", "required");

        // Act
        var result = await controller.Login(new LoginDto
        {
            Email = "test@example.com",
            Password = "Password123!"
        });

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task Login_ShouldReturnBadRequest_WhenCredentialsAreInvalid()
    {
        // Arrange
        var userManagerMock = GetUserManagerMock();
        userManagerMock.Setup(manager => manager.FindByEmailAsync("test@example.com")).ReturnsAsync((User?)null);
        var controller = CreateController(userManagerMock.Object, new Mock<ITokenService>().Object, true);

        // Act
        var result = await controller.Login(new LoginDto
        {
            Email = "test@example.com",
            Password = "Password123!"
        });

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }
    #endregion

    #region Logout
    [Fact]
    public void Logout_ShouldReturnOkAndDeleteCookieWithProductionOptions_WhenEnvironmentIsProduction()
    {
        // Arrange
        var controller = CreateController(GetUserManagerMock().Object, new Mock<ITokenService>().Object, true);

        // Act
        var result = controller.Logout();

        // Assert
        Assert.IsType<OkResult>(result);
        var header = controller.Response.Headers.SetCookie.ToString().ToLowerInvariant();
        Assert.Contains("token=", header);
        Assert.Contains("secure", header);
        Assert.Contains("samesite=none", header);
    }

    [Fact]
    public void Logout_ShouldReturnOkAndDeleteCookieWithDevelopmentOptions_WhenEnvironmentIsNotProduction()
    {
        // Arrange
        var controller = CreateController(GetUserManagerMock().Object, new Mock<ITokenService>().Object, false);

        // Act
        var result = controller.Logout();

        // Assert
        Assert.IsType<OkResult>(result);
        var header = controller.Response.Headers.SetCookie.ToString().ToLowerInvariant();
        Assert.Contains("token=", header);
        Assert.Contains("samesite=lax", header);
    }
    #endregion

    private static AccountController CreateController(UserManager<User> userManager, ITokenService tokenService, bool isProduction)
    {
        var loggerMock = new Mock<ILogger<AccountController>>();
        var envMock = new Mock<IWebHostEnvironment>();
        envMock.SetupGet(env => env.EnvironmentName).Returns(isProduction ? "Production" : "Development");

        return new AccountController(userManager, new UserMapper(), loggerMock.Object, tokenService, envMock.Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            }
        };
    }

    private static Mock<UserManager<User>> GetUserManagerMock()
    {
        return new Mock<UserManager<User>>(
            Mock.Of<IUserStore<User>>(), null!, null!, null!, null!, null!, null!, null!, null!);
    }
}