using Microsoft.AspNetCore.Mvc;
using mmotors_back.Features.Applications.Controllers;
using mmotors_back.Features.Applications.Dtos;
using mmotors_back.Features.Applications.Interfaces;
using mmotors_back.Features.Applications.Services;
using mmotors_back.Models;
using mmotors_back.Tests.Helpers;
using Moq;

namespace mmotors_back.Tests.Features.Applications;

public class ApplicationsControllerTests
{
    #region CreateApplication
    [Fact]
    public async Task CreateApplication_ShouldReturnCreatedAtAction_WhenRequestIsValid()
    {
        // Arrange
        var createdApplication = new ApplicationDto { Id = 4, UserId = Guid.NewGuid().ToString(), VehicleId = 2 };
        var repositoryMock = new Mock<IApplicationsRepository>();
        repositoryMock.Setup(repository => repository.CreateApplicationAsync(It.IsAny<CreateApplicationDto>())).ReturnsAsync(createdApplication);
        var controller = CreateController(repositoryMock.Object);

        // Act
        var result = await controller.CreateApplication(new CreateApplicationDto { UserId = createdApplication.UserId, VehicleId = createdApplication.VehicleId });

        // Assert
        var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result);
        Assert.Equal(createdApplication.Id, ((ApplicationDto)createdAtActionResult.Value!).Id);
    }

    [Fact]
    public async Task CreateApplication_ShouldReturnBadRequest_WhenModelStateIsInvalid()
    {
        // Arrange
        var repositoryMock = new Mock<IApplicationsRepository>();
        var controller = CreateController(repositoryMock.Object);
        controller.ModelState.AddModelError("VehicleId", "required");

        // Act
        var result = await controller.CreateApplication(new CreateApplicationDto());

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task CreateApplication_ShouldReturnNotFound_WhenVehicleDoesNotExist()
    {
        // Arrange
        var repositoryMock = new Mock<IApplicationsRepository>();
        repositoryMock.Setup(repository => repository.CreateApplicationAsync(It.IsAny<CreateApplicationDto>())).ThrowsAsync(new KeyNotFoundException("missing"));
        var controller = CreateController(repositoryMock.Object);

        // Act
        var result = await controller.CreateApplication(new CreateApplicationDto());

        // Assert
        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task CreateApplication_ShouldReturnBadRequest_WhenVehicleIsUnavailable()
    {
        // Arrange
        var repositoryMock = new Mock<IApplicationsRepository>();
        repositoryMock.Setup(repository => repository.CreateApplicationAsync(It.IsAny<CreateApplicationDto>())).ThrowsAsync(new InvalidOperationException("unavailable"));
        var controller = CreateController(repositoryMock.Object);

        // Act
        var result = await controller.CreateApplication(new CreateApplicationDto());

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }
    #endregion

    #region GetApplicationById
    [Fact]
    public async Task GetApplicationById_ShouldReturnOk_WhenAuthorizedUserRequestsExistingApplication()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        var application = new ApplicationDto { Id = 8, UserId = userId, VehicleId = 3 };
        var repositoryMock = new Mock<IApplicationsRepository>();
        repositoryMock.Setup(repository => repository.GetApplicationByIdAsync(8, null)).ReturnsAsync(application);
        var controller = CreateController(repositoryMock.Object);
        TestClaimsPrincipalFactory.AttachUser(controller, TestClaimsPrincipalFactory.CreateCustomer(userId));

        // Act
        var result = await controller.GetApplicationById(8);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(application.Id, ((ApplicationDto)okResult.Value!).Id);
    }

    [Fact]
    public async Task GetApplicationById_ShouldReturnBadRequest_WhenIdIsInvalid()
    {
        // Arrange
        var repositoryMock = new Mock<IApplicationsRepository>();
        var controller = CreateController(repositoryMock.Object);

        // Act
        var result = await controller.GetApplicationById(0);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task GetApplicationById_ShouldReturnAuthorizationResult_WhenCustomerIsNotAuthorized()
    {
        // Arrange
        var ownerId = Guid.NewGuid().ToString();
        var requestUserId = Guid.NewGuid().ToString();
        var application = new ApplicationDto { Id = 8, UserId = ownerId, VehicleId = 3 };
        var repositoryMock = new Mock<IApplicationsRepository>();
        repositoryMock.Setup(repository => repository.GetApplicationByIdAsync(8, null)).ReturnsAsync(application);
        var controller = CreateController(repositoryMock.Object);
        TestClaimsPrincipalFactory.AttachUser(controller, TestClaimsPrincipalFactory.CreateCustomer(requestUserId));

        // Act
        var result = await controller.GetApplicationById(8);

        // Assert
        Assert.IsType<ForbidResult>(result);
    }

    [Fact]
    public async Task GetApplicationById_ShouldReturnNotFound_WhenRepositoryThrowsKeyNotFound()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        var repositoryMock = new Mock<IApplicationsRepository>();
        repositoryMock.Setup(repository => repository.GetApplicationByIdAsync(13, null)).ThrowsAsync(new KeyNotFoundException());
        var controller = CreateController(repositoryMock.Object);
        TestClaimsPrincipalFactory.AttachUser(controller, TestClaimsPrincipalFactory.CreateAdmin(userId));

        // Act
        var result = await controller.GetApplicationById(13);

        // Assert
        Assert.IsType<NotFoundObjectResult>(result);
    }
    #endregion

    #region GetAllApplications
    [Fact]
    public async Task GetAllApplications_ShouldReturnOkWithPagedResults_WhenRepositoryReturnsResults()
    {
        // Arrange
        var repositoryMock = new Mock<IApplicationsRepository>();
        repositoryMock.Setup(repository => repository.GetAllApplicationsAsync(It.IsAny<PaginationParams>(), It.IsAny<System.Security.Claims.ClaimsPrincipal>()))
            .ReturnsAsync(new PagedResults<ApplicationDto>
            {
                Items = new[] { new ApplicationDto { Id = 1, UserId = Guid.NewGuid().ToString(), VehicleId = 3 } },
                TotalCount = 1,
                PageNumber = 1,
                PageSize = 10
            });
        var controller = CreateController(repositoryMock.Object);
        TestClaimsPrincipalFactory.AttachUser(controller, TestClaimsPrincipalFactory.CreateAdmin(Guid.NewGuid().ToString()));

        // Act
        var result = await controller.GetAllApplications(new PaginationParams { PageNumber = 1, PageSize = 10 });

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.IsType<PagedResults<ApplicationDto>>(okResult.Value);
    }
    #endregion

    #region DeleteApplication
    [Fact]
    public async Task DeleteApplication_ShouldReturnNoContent_WhenDeletionSucceeds()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        var application = new ApplicationDto { Id = 6, UserId = userId, VehicleId = 1 };
        var repositoryMock = new Mock<IApplicationsRepository>();
        repositoryMock.Setup(repository => repository.GetApplicationByIdAsync(6, null)).ReturnsAsync(application);
        repositoryMock.Setup(repository => repository.DeleteApplicationAsync(6, It.IsAny<System.Security.Claims.ClaimsPrincipal>())).Returns(Task.CompletedTask);
        var controller = CreateController(repositoryMock.Object);
        TestClaimsPrincipalFactory.AttachUser(controller, TestClaimsPrincipalFactory.CreateCustomer(userId));

        // Act
        var result = await controller.DeleteApplication(6);

        // Assert
        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task DeleteApplication_ShouldReturnBadRequest_WhenIdIsInvalid()
    {
        // Arrange
        var repositoryMock = new Mock<IApplicationsRepository>();
        var controller = CreateController(repositoryMock.Object);

        // Act
        var result = await controller.DeleteApplication(0);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task DeleteApplication_ShouldReturnAuthorizationResult_WhenCustomerIsNotAuthorized()
    {
        // Arrange
        var application = new ApplicationDto { Id = 6, UserId = Guid.NewGuid().ToString(), VehicleId = 1 };
        var repositoryMock = new Mock<IApplicationsRepository>();
        repositoryMock.Setup(repository => repository.GetApplicationByIdAsync(6, null)).ReturnsAsync(application);
        var controller = CreateController(repositoryMock.Object);
        TestClaimsPrincipalFactory.AttachUser(controller, TestClaimsPrincipalFactory.CreateCustomer(Guid.NewGuid().ToString()));

        // Act
        var result = await controller.DeleteApplication(6);

        // Assert
        Assert.IsType<ForbidResult>(result);
    }

    [Fact]
    public async Task DeleteApplication_ShouldReturnNotFound_WhenRepositoryThrowsKeyNotFound()
    {
        // Arrange
        var repositoryMock = new Mock<IApplicationsRepository>();
        repositoryMock.Setup(repository => repository.DeleteApplicationAsync(6, It.IsAny<System.Security.Claims.ClaimsPrincipal>())).ThrowsAsync(new KeyNotFoundException());
        var controller = CreateController(repositoryMock.Object);
        TestClaimsPrincipalFactory.AttachUser(controller, TestClaimsPrincipalFactory.CreateAdmin(Guid.NewGuid().ToString()));

        // Act
        var result = await controller.DeleteApplication(6);

        // Assert
        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task DeleteApplication_ShouldReturnBadRequest_WhenRepositoryThrowsInvalidOperation()
    {
        // Arrange
        var repositoryMock = new Mock<IApplicationsRepository>();
        repositoryMock.Setup(repository => repository.DeleteApplicationAsync(6, It.IsAny<System.Security.Claims.ClaimsPrincipal>())).ThrowsAsync(new InvalidOperationException());
        var controller = CreateController(repositoryMock.Object);
        TestClaimsPrincipalFactory.AttachUser(controller, TestClaimsPrincipalFactory.CreateAdmin(Guid.NewGuid().ToString()));

        // Act
        var result = await controller.DeleteApplication(6);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }
    #endregion

    #region SubmitApplication
    [Fact]
    public async Task SubmitApplication_ShouldReturnOk_WhenSubmissionSucceeds()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        var application = new ApplicationDto { Id = 9, UserId = userId, VehicleId = 3 };
        var repositoryMock = new Mock<IApplicationsRepository>();
        repositoryMock.Setup(repository => repository.GetApplicationByIdAsync(9, null)).ReturnsAsync(application);
        repositoryMock.Setup(repository => repository.SubmitApplicationAsync(9, It.IsAny<System.Security.Claims.ClaimsPrincipal>())).Returns(Task.CompletedTask);
        var controller = CreateController(repositoryMock.Object);
        TestClaimsPrincipalFactory.AttachUser(controller, TestClaimsPrincipalFactory.CreateCustomer(userId));

        // Act
        var result = await controller.SubmitApplication(9);

        // Assert
        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task SubmitApplication_ShouldReturnBadRequest_WhenIdIsInvalid()
    {
        // Arrange
        var repositoryMock = new Mock<IApplicationsRepository>();
        var controller = CreateController(repositoryMock.Object);

        // Act
        var result = await controller.SubmitApplication(0);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task SubmitApplication_ShouldReturnAuthorizationResult_WhenCustomerIsNotAuthorized()
    {
        // Arrange
        var application = new ApplicationDto { Id = 9, UserId = Guid.NewGuid().ToString(), VehicleId = 3 };
        var repositoryMock = new Mock<IApplicationsRepository>();
        repositoryMock.Setup(repository => repository.GetApplicationByIdAsync(9, null)).ReturnsAsync(application);
        var controller = CreateController(repositoryMock.Object);
        TestClaimsPrincipalFactory.AttachUser(controller, TestClaimsPrincipalFactory.CreateCustomer(Guid.NewGuid().ToString()));

        // Act
        var result = await controller.SubmitApplication(9);

        // Assert
        Assert.IsType<ForbidResult>(result);
    }

    [Fact]
    public async Task SubmitApplication_ShouldReturnNotFound_WhenRepositoryThrowsKeyNotFound()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        var application = new ApplicationDto { Id = 9, UserId = userId, VehicleId = 3 };
        var repositoryMock = new Mock<IApplicationsRepository>();
        repositoryMock.Setup(repository => repository.GetApplicationByIdAsync(9, null)).ReturnsAsync(application);
        repositoryMock.Setup(repository => repository.SubmitApplicationAsync(9, It.IsAny<System.Security.Claims.ClaimsPrincipal>())).ThrowsAsync(new KeyNotFoundException());
        var controller = CreateController(repositoryMock.Object);
        TestClaimsPrincipalFactory.AttachUser(controller, TestClaimsPrincipalFactory.CreateCustomer(userId));

        // Act
        var result = await controller.SubmitApplication(9);

        // Assert
        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task SubmitApplication_ShouldReturnBadRequest_WhenRepositoryThrowsInvalidOperation()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        var application = new ApplicationDto { Id = 9, UserId = userId, VehicleId = 3 };
        var repositoryMock = new Mock<IApplicationsRepository>();
        repositoryMock.Setup(repository => repository.GetApplicationByIdAsync(9, null)).ReturnsAsync(application);
        repositoryMock.Setup(repository => repository.SubmitApplicationAsync(9, It.IsAny<System.Security.Claims.ClaimsPrincipal>())).ThrowsAsync(new InvalidOperationException());
        var controller = CreateController(repositoryMock.Object);
        TestClaimsPrincipalFactory.AttachUser(controller, TestClaimsPrincipalFactory.CreateCustomer(userId));

        // Act
        var result = await controller.SubmitApplication(9);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }
    #endregion

    #region HoldApplication
    [Fact]
    public async Task HoldApplication_ShouldReturnOk_WhenHoldSucceeds()
    {
        // Arrange
        var repositoryMock = new Mock<IApplicationsRepository>();
        repositoryMock.Setup(repository => repository.HoldApplicationAsync(12, It.IsAny<System.Security.Claims.ClaimsPrincipal>())).Returns(Task.CompletedTask);
        var controller = CreateController(repositoryMock.Object);
        TestClaimsPrincipalFactory.AttachUser(controller, TestClaimsPrincipalFactory.CreateStaff(Guid.NewGuid().ToString()));

        // Act
        var result = await controller.HoldApplication(12);

        // Assert
        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task HoldApplication_ShouldReturnBadRequest_WhenIdIsInvalid()
    {
        // Arrange
        var repositoryMock = new Mock<IApplicationsRepository>();
        var controller = CreateController(repositoryMock.Object);

        // Act
        var result = await controller.HoldApplication(0);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task HoldApplication_ShouldReturnNotFound_WhenRepositoryThrowsKeyNotFound()
    {
        // Arrange
        var repositoryMock = new Mock<IApplicationsRepository>();
        repositoryMock.Setup(repository => repository.HoldApplicationAsync(12, It.IsAny<System.Security.Claims.ClaimsPrincipal>())).ThrowsAsync(new KeyNotFoundException());
        var controller = CreateController(repositoryMock.Object);
        TestClaimsPrincipalFactory.AttachUser(controller, TestClaimsPrincipalFactory.CreateStaff(Guid.NewGuid().ToString()));

        // Act
        var result = await controller.HoldApplication(12);

        // Assert
        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task HoldApplication_ShouldReturnBadRequest_WhenRepositoryThrowsInvalidOperation()
    {
        // Arrange
        var repositoryMock = new Mock<IApplicationsRepository>();
        repositoryMock.Setup(repository => repository.HoldApplicationAsync(12, It.IsAny<System.Security.Claims.ClaimsPrincipal>())).ThrowsAsync(new InvalidOperationException());
        var controller = CreateController(repositoryMock.Object);
        TestClaimsPrincipalFactory.AttachUser(controller, TestClaimsPrincipalFactory.CreateStaff(Guid.NewGuid().ToString()));

        // Act
        var result = await controller.HoldApplication(12);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }
    #endregion

    #region ReviewApplication
    [Fact]
    public async Task ReviewApplication_ShouldReturnOk_WhenReviewSucceeds()
    {
        // Arrange
        var reviewApplication = new ReviewApplicationDto { ApplicationId = 4, IsApproved = true };
        var repositoryMock = new Mock<IApplicationsRepository>();
        repositoryMock.Setup(repository => repository.ReviewApplicationAsync(reviewApplication, It.IsAny<System.Security.Claims.ClaimsPrincipal>())).Returns(Task.CompletedTask);
        var controller = CreateController(repositoryMock.Object);
        TestClaimsPrincipalFactory.AttachUser(controller, TestClaimsPrincipalFactory.CreateStaff(Guid.NewGuid().ToString()));

        // Act
        var result = await controller.ReviewApplication(4, reviewApplication);

        // Assert
        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task ReviewApplication_ShouldReturnBadRequest_WhenIdIsInvalid()
    {
        // Arrange
        var repositoryMock = new Mock<IApplicationsRepository>();
        var controller = CreateController(repositoryMock.Object);

        // Act
        var result = await controller.ReviewApplication(0, new ReviewApplicationDto());

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task ReviewApplication_ShouldReturnNotFound_WhenRepositoryThrowsKeyNotFound()
    {
        // Arrange
        var reviewApplication = new ReviewApplicationDto { ApplicationId = 4, IsApproved = true };
        var repositoryMock = new Mock<IApplicationsRepository>();
        repositoryMock.Setup(repository => repository.ReviewApplicationAsync(reviewApplication, It.IsAny<System.Security.Claims.ClaimsPrincipal>())).ThrowsAsync(new KeyNotFoundException());
        var controller = CreateController(repositoryMock.Object);
        TestClaimsPrincipalFactory.AttachUser(controller, TestClaimsPrincipalFactory.CreateStaff(Guid.NewGuid().ToString()));

        // Act
        var result = await controller.ReviewApplication(4, reviewApplication);

        // Assert
        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task ReviewApplication_ShouldReturnBadRequest_WhenRepositoryThrowsInvalidOperation()
    {
        // Arrange
        var reviewApplication = new ReviewApplicationDto { ApplicationId = 4, IsApproved = true };
        var repositoryMock = new Mock<IApplicationsRepository>();
        repositoryMock.Setup(repository => repository.ReviewApplicationAsync(reviewApplication, It.IsAny<System.Security.Claims.ClaimsPrincipal>())).ThrowsAsync(new InvalidOperationException());
        var controller = CreateController(repositoryMock.Object);
        TestClaimsPrincipalFactory.AttachUser(controller, TestClaimsPrincipalFactory.CreateStaff(Guid.NewGuid().ToString()));

        // Act
        var result = await controller.ReviewApplication(4, reviewApplication);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }
    #endregion

    private static ApplicationsController CreateController(IApplicationsRepository repository)
    {
        return new ApplicationsController(repository, new CheckAuthorization(repository));
    }
}