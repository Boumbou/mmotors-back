using Microsoft.AspNetCore.Mvc;
using mmotors_back.Features.Services.Controllers;
using mmotors_back.Features.Services.Dtos;
using mmotors_back.Features.Services.Interfaces;
using mmotors_back.Models;
using Moq;

namespace mmotors_back.Tests.Features.Services;

public class ServicesControllerTests
{
    #region GetServices
    [Fact]
    public async Task GetServices_ShouldReturnOkWithServices_WhenRepositoryReturnsResults()
    {
        // Arrange
        var expectedServices = new List<ServiceDto>
        {
            new() { Id = 1, Name = "Registration", Description = "Admin work", ListingType = ListingType.SALE },
            new() { Id = 2, Name = "Insurance", Description = "Coverage", ListingType = ListingType.RENTAL }
        };
        var repositoryMock = new Mock<IServicesRepository>();
        repositoryMock.Setup(repository => repository.GetAllServicesAsync(null)).ReturnsAsync(expectedServices);
        var controller = new ServicesController(repositoryMock.Object);

        // Act
        var result = await controller.GetServices();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var services = Assert.IsAssignableFrom<IEnumerable<ServiceDto>>(okResult.Value);
        Assert.Equal(2, services.Count());
    }

    [Fact]
    public async Task GetServices_ShouldPassListingTypeToRepository_WhenFilterIsProvided()
    {
        // Arrange
        var repositoryMock = new Mock<IServicesRepository>();
        repositoryMock.Setup(repository => repository.GetAllServicesAsync(ListingType.SALE)).ReturnsAsync(new List<ServiceDto>());
        var controller = new ServicesController(repositoryMock.Object);

        // Act
        var result = await controller.GetServices(ListingType.SALE);

        // Assert
        Assert.IsType<OkObjectResult>(result.Result);
        repositoryMock.Verify(repository => repository.GetAllServicesAsync(ListingType.SALE), Times.Once);
    }
    #endregion

    #region GetServiceById
    [Fact]
    public async Task GetServiceById_ShouldReturnOk_WhenServiceExists()
    {
        // Arrange
        var service = new ServiceDto { Id = 4, Name = "Delivery", Description = "Home delivery", ListingType = ListingType.SALE };
        var repositoryMock = new Mock<IServicesRepository>();
        repositoryMock.Setup(repository => repository.GetServiceByIdAsync(4)).ReturnsAsync(service);
        var controller = new ServicesController(repositoryMock.Object);

        // Act
        var result = await controller.GetServiceById(4);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedService = Assert.IsType<ServiceDto>(okResult.Value);
        Assert.Equal(service.Id, returnedService.Id);
    }

    [Fact]
    public async Task GetServiceById_ShouldReturnNotFound_WhenRepositoryThrowsKeyNotFound()
    {
        // Arrange
        var repositoryMock = new Mock<IServicesRepository>();
        repositoryMock.Setup(repository => repository.GetServiceByIdAsync(99)).ThrowsAsync(new KeyNotFoundException());
        var controller = new ServicesController(repositoryMock.Object);

        // Act
        var result = await controller.GetServiceById(99);

        // Assert
        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task GetServiceById_ShouldReturnInternalServerError_WhenRepositoryThrowsUnexpectedException()
    {
        // Arrange
        var repositoryMock = new Mock<IServicesRepository>();
        repositoryMock.Setup(repository => repository.GetServiceByIdAsync(5)).ThrowsAsync(new Exception("boom"));
        var controller = new ServicesController(repositoryMock.Object);

        // Act
        var result = await controller.GetServiceById(5);

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(500, objectResult.StatusCode);
    }
    #endregion

    #region CreateService
    [Fact]
    public async Task CreateService_ShouldReturnCreatedAtAction_WhenServiceIsCreated()
    {
        // Arrange
        var createDto = new CreateServiceDto { Name = "Extension", Description = "Warranty extension", ListingType = ListingType.SALE };
        var createdService = new ServiceDto { Id = 6, Name = "Extension", Description = "Warranty extension", ListingType = ListingType.SALE };
        var repositoryMock = new Mock<IServicesRepository>();
        repositoryMock.Setup(repository => repository.CreateServiceAsync(createDto)).ReturnsAsync(createdService);
        var controller = new ServicesController(repositoryMock.Object);

        // Act
        var result = await controller.CreateService(createDto);

        // Assert
        var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result.Result);
        Assert.Equal(nameof(ServicesController.GetServiceById), createdAtActionResult.ActionName);
        Assert.Equal(createdService.Id, ((ServiceDto)createdAtActionResult.Value!).Id);
    }
    #endregion

    #region UpdateService
    [Fact]
    public async Task UpdateService_ShouldReturnNoContent_WhenUpdateSucceeds()
    {
        // Arrange
        var service = new ServiceDto { Id = 3, Name = "Assistance", Description = "Roadside", ListingType = ListingType.RENTAL };
        var repositoryMock = new Mock<IServicesRepository>();
        repositoryMock.Setup(repository => repository.UpdateServiceAsync(service)).ReturnsAsync(service.Id);
        var controller = new ServicesController(repositoryMock.Object);

        // Act
        var result = await controller.UpdateService(service.Id, service);

        // Assert
        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task UpdateService_ShouldReturnBadRequest_WhenModelStateIsInvalid()
    {
        // Arrange
        var service = new ServiceDto { Id = 3, Name = "Assistance", Description = "Roadside", ListingType = ListingType.RENTAL };
        var repositoryMock = new Mock<IServicesRepository>();
        var controller = new ServicesController(repositoryMock.Object);
        controller.ModelState.AddModelError("Name", "required");

        // Act
        var result = await controller.UpdateService(service.Id, service);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task UpdateService_ShouldReturnBadRequest_WhenRouteIdDoesNotMatchDtoId()
    {
        // Arrange
        var service = new ServiceDto { Id = 3, Name = "Assistance", Description = "Roadside", ListingType = ListingType.RENTAL };
        var repositoryMock = new Mock<IServicesRepository>();
        var controller = new ServicesController(repositoryMock.Object);

        // Act
        var result = await controller.UpdateService(8, service);

        // Assert
        Assert.IsType<BadRequestResult>(result);
    }

    [Fact]
    public async Task UpdateService_ShouldReturnNotFound_WhenRepositoryReturnsZero()
    {
        // Arrange
        var service = new ServiceDto { Id = 3, Name = "Assistance", Description = "Roadside", ListingType = ListingType.RENTAL };
        var repositoryMock = new Mock<IServicesRepository>();
        repositoryMock.Setup(repository => repository.UpdateServiceAsync(service)).ReturnsAsync(0);
        var controller = new ServicesController(repositoryMock.Object);

        // Act
        var result = await controller.UpdateService(service.Id, service);

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }
    #endregion

    #region DeleteService
    [Fact]
    public async Task DeleteService_ShouldReturnNoContent_WhenDeleteSucceeds()
    {
        // Arrange
        var repositoryMock = new Mock<IServicesRepository>();
        repositoryMock.Setup(repository => repository.DeleteServiceAsync(5)).ReturnsAsync(5);
        var controller = new ServicesController(repositoryMock.Object);

        // Act
        var result = await controller.DeleteService(5);

        // Assert
        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task DeleteService_ShouldReturnNotFound_WhenRepositoryReturnsZero()
    {
        // Arrange
        var repositoryMock = new Mock<IServicesRepository>();
        repositoryMock.Setup(repository => repository.DeleteServiceAsync(7)).ReturnsAsync(0);
        var controller = new ServicesController(repositoryMock.Object);

        // Act
        var result = await controller.DeleteService(7);

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }
    #endregion

    #region ToggleService
    [Fact]
    public async Task ToggleService_ShouldReturnNoContent_WhenToggleSucceeds()
    {
        // Arrange
        var repositoryMock = new Mock<IServicesRepository>();
        repositoryMock.Setup(repository => repository.ToggleServiceStatusAsync(6)).ReturnsAsync(6);
        var controller = new ServicesController(repositoryMock.Object);

        // Act
        var result = await controller.ToggleService(6);

        // Assert
        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task ToggleService_ShouldReturnNotFound_WhenRepositoryReturnsZero()
    {
        // Arrange
        var repositoryMock = new Mock<IServicesRepository>();
        repositoryMock.Setup(repository => repository.ToggleServiceStatusAsync(6)).ReturnsAsync(0);
        var controller = new ServicesController(repositoryMock.Object);

        // Act
        var result = await controller.ToggleService(6);

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }
    #endregion
}