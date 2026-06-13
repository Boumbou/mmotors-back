using Microsoft.AspNetCore.Mvc;
using mmotors_back.Features.Applications.Dtos;
using mmotors_back.Features.Applications.Interfaces;
using mmotors_back.Features.Applications.Services;
using mmotors_back.Models;
using mmotors_back.Tests.Helpers;
using Moq;

namespace mmotors_back.Tests.Features.Applications;

public class CheckAuthorizationTests
{
    #region IsUserAuthorized
    [Fact]
    public async Task IsUserAuthorized_ShouldReturnOk_WhenApplicationBelongsToCurrentUser()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        var repositoryMock = new Mock<IApplicationsRepository>();
        repositoryMock
            .Setup(repository => repository.GetApplicationByIdAsync(10, null))
            .ReturnsAsync(new ApplicationDto
            {
                Id = 10,
                UserId = userId,
                VehicleId = 1
            });

        var service = new CheckAuthorization(repositoryMock.Object);
        var user = TestClaimsPrincipalFactory.CreateCustomer(userId);

        // Act
        var result = await service.IsUserAuthorized(user, 10);

        // Assert
        Assert.IsType<OkResult>(result);
    }

    [Fact]
    public async Task IsUserAuthorized_ShouldReturnNotFound_WhenApplicationDoesNotExist()
    {
        // Arrange
        var repositoryMock = new Mock<IApplicationsRepository>();
        repositoryMock
            .Setup(repository => repository.GetApplicationByIdAsync(44, null))
            .ThrowsAsync(new KeyNotFoundException());

        var service = new CheckAuthorization(repositoryMock.Object);
        var user = TestClaimsPrincipalFactory.CreateCustomer(Guid.NewGuid().ToString());

        // Act
        var result = await service.IsUserAuthorized(user, 44);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Contains("44", notFoundResult.Value!.ToString());
    }

    [Fact]
    public async Task IsUserAuthorized_ShouldReturnForbid_WhenUserIdIsMissingOrDoesNotMatch()
    {
        // Arrange
        var ownerId = Guid.NewGuid().ToString();
        var repositoryMock = new Mock<IApplicationsRepository>();
        repositoryMock
            .Setup(repository => repository.GetApplicationByIdAsync(7, null))
            .ReturnsAsync(new ApplicationDto
            {
                Id = 7,
                UserId = ownerId,
                VehicleId = 2
            });

        var service = new CheckAuthorization(repositoryMock.Object);
        var otherUser = TestClaimsPrincipalFactory.CreateCustomer(Guid.NewGuid().ToString());

        // Act
        var result = await service.IsUserAuthorized(otherUser, 7);

        // Assert
        Assert.IsType<ForbidResult>(result);
    }
    #endregion
}