using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using mmotors_back.Features.Shared.Interfaces;
using mmotors_back.Features.Vehicles.Controllers;
using mmotors_back.Features.Vehicles.Dtos;
using mmotors_back.Features.Vehicles.Interfaces;
using mmotors_back.Models;
using mmotors_back.Tests.Helpers;
using Moq;

namespace mmotors_back.Tests.Features.Vehicles;

public class VehiclesControllerTests
{
    #region GetAllVehicles
    [Fact]
    public async Task GetAllVehicles_ShouldReturnOkAndRewriteImageUrls_WhenVehiclesHaveImageKeys()
    {
        // Arrange
        var vehicles = new PagedResults<VehicleDto>
        {
            Items = new[]
            {
                new VehicleDto
                {
                    Id = 1,
                    Brand = "Peugeot",
                    Model = "3008",
                    ListingType = ListingType.SALE,
                    Status = VehicleStatus.AVAILABLE,
                    ImageKey = "vehicles/3008.jpg"
                }
            },
            TotalCount = 1,
            PageNumber = 1,
            PageSize = 10
        };
        var repositoryMock = new Mock<IVehiclesRepository>();
        repositoryMock.Setup(repository => repository.GetAllVehiclesAsync(null, It.IsAny<PaginationParams>())).ReturnsAsync(vehicles);
        var storageMock = new Mock<IStorageService>();
        storageMock.Setup(storage => storage.GetFileUrl("vehicles/3008.jpg", "01_vehicules")).Returns("https://cdn.example.com/3008.jpg");
        var controller = new VehiclesController(repositoryMock.Object, storageMock.Object);

        // Act
        var result = await controller.GetAllVehicles();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedVehicles = Assert.IsType<PagedResults<VehicleDto>>(okResult.Value);
        Assert.Equal("https://cdn.example.com/3008.jpg", returnedVehicles.Items.Single().ImageUrl);
    }

    [Fact]
    public async Task GetAllVehicles_ShouldLeaveImageUrlUnchanged_WhenVehicleHasNoImageKey()
    {
        // Arrange
        var vehicles = new PagedResults<VehicleDto>
        {
            Items = new[]
            {
                new VehicleDto
                {
                    Id = 2,
                    Brand = "Renault",
                    Model = "Captur",
                    ListingType = ListingType.RENTAL,
                    Status = VehicleStatus.AVAILABLE,
                    ImageUrl = "existing-url"
                }
            },
            TotalCount = 1,
            PageNumber = 1,
            PageSize = 10
        };
        var repositoryMock = new Mock<IVehiclesRepository>();
        repositoryMock.Setup(repository => repository.GetAllVehiclesAsync(null, It.IsAny<PaginationParams>())).ReturnsAsync(vehicles);
        var storageMock = new Mock<IStorageService>();
        var controller = new VehiclesController(repositoryMock.Object, storageMock.Object);

        // Act
        var result = await controller.GetAllVehicles();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedVehicles = Assert.IsType<PagedResults<VehicleDto>>(okResult.Value);
        Assert.Equal("existing-url", returnedVehicles.Items.Single().ImageUrl);
        storageMock.Verify(storage => storage.GetFileUrl(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task GetAllVehicles_ShouldUseDefaultPagination_WhenPaginationIsNull()
    {
        // Arrange
        var repositoryMock = new Mock<IVehiclesRepository>();
        repositoryMock.Setup(repository => repository.GetAllVehiclesAsync("sale", It.IsAny<PaginationParams>()))
            .ReturnsAsync(new PagedResults<VehicleDto>
            {
                Items = Array.Empty<VehicleDto>(),
                TotalCount = 0,
                PageNumber = 1,
                PageSize = 10
            });
        var storageMock = new Mock<IStorageService>();
        var controller = new VehiclesController(repositoryMock.Object, storageMock.Object);

        // Act
        var result = await controller.GetAllVehicles("sale", null);

        // Assert
        Assert.IsType<OkObjectResult>(result);
        repositoryMock.Verify(repository => repository.GetAllVehiclesAsync("sale", It.Is<PaginationParams>(pagination => pagination.PageNumber == 1 && pagination.PageSize == 10)), Times.Once);
    }
    #endregion

    #region GetVehicleById
    [Fact]
    public async Task GetVehicleById_ShouldReturnOkAndRewriteImageUrl_WhenVehicleExists()
    {
        // Arrange
        var vehicle = new VehicleDto
        {
            Id = 3,
            Brand = "Audi",
            Model = "Q4",
            ListingType = ListingType.SALE,
            Status = VehicleStatus.AVAILABLE,
            ImageKey = "vehicles/q4.jpg"
        };
        var repositoryMock = new Mock<IVehiclesRepository>();
        repositoryMock.Setup(repository => repository.GetVehicleByIdAsync(3)).ReturnsAsync(vehicle);
        var storageMock = new Mock<IStorageService>();
        storageMock.Setup(storage => storage.GetFileUrl("vehicles/q4.jpg", "01_vehicules")).Returns("https://cdn.example.com/q4.jpg");
        var controller = new VehiclesController(repositoryMock.Object, storageMock.Object);

        // Act
        var result = await controller.GetVehicleById(3);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedVehicle = Assert.IsType<VehicleDto>(okResult.Value);
        Assert.Equal("https://cdn.example.com/q4.jpg", returnedVehicle.ImageUrl);
    }

    [Fact]
    public async Task GetVehicleById_ShouldReturnNotFound_WhenRepositoryThrowsKeyNotFound()
    {
        // Arrange
        var repositoryMock = new Mock<IVehiclesRepository>();
        repositoryMock.Setup(repository => repository.GetVehicleByIdAsync(33)).ThrowsAsync(new KeyNotFoundException());
        var storageMock = new Mock<IStorageService>();
        var controller = new VehiclesController(repositoryMock.Object, storageMock.Object);

        // Act
        var result = await controller.GetVehicleById(33);

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }
    #endregion

    #region AddVehicle
    [Fact]
    public async Task AddVehicle_ShouldReturnCreatedAtAction_WhenVehicleIsValidAndNoImageIsProvided()
    {
        // Arrange
        var createVehicleDto = CreateCreateVehicleDto();
        var createdVehicle = new VehicleDto
        {
            Id = 11,
            Brand = createVehicleDto.Brand,
            Model = createVehicleDto.Model,
            Year = createVehicleDto.Year,
            Motorization = createVehicleDto.Motorization,
            Mileage = createVehicleDto.Mileage,
            ListedAmount = createVehicleDto.ListedAmount,
            ListingType = createVehicleDto.ListingType,
            Status = VehicleStatus.AVAILABLE
        };
        var repositoryMock = new Mock<IVehiclesRepository>();
        repositoryMock.Setup(repository => repository.AddVehicleAsync(createVehicleDto, It.IsAny<System.Security.Claims.ClaimsPrincipal>())).ReturnsAsync(createdVehicle);
        var storageMock = new Mock<IStorageService>();
        var controller = new VehiclesController(repositoryMock.Object, storageMock.Object);
        TestClaimsPrincipalFactory.AttachUser(controller, TestClaimsPrincipalFactory.CreateStaff(Guid.NewGuid().ToString()));

        // Act
        var result = await controller.AddVehicle(createVehicleDto, null);

        // Assert
        var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result);
        Assert.Equal(createdVehicle.Id, ((VehicleDto)createdAtActionResult.Value!).Id);
    }

    [Fact]
    public async Task AddVehicle_ShouldReturnBadRequest_WhenModelStateIsInvalid()
    {
        // Arrange
        var repositoryMock = new Mock<IVehiclesRepository>();
        var storageMock = new Mock<IStorageService>();
        var controller = new VehiclesController(repositoryMock.Object, storageMock.Object);
        controller.ModelState.AddModelError("Brand", "required");

        // Act
        var result = await controller.AddVehicle(CreateCreateVehicleDto(), null);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task AddVehicle_ShouldUploadImageAndPassUpdatedDto_WhenImageIsProvided()
    {
        // Arrange
        var createVehicleDto = CreateCreateVehicleDto();
        var imageMock = CreateFormFileMock("gla.jpg", "image/jpeg", 512);
        var repositoryMock = new Mock<IVehiclesRepository>();
        repositoryMock.Setup(repository => repository.AddVehicleAsync(It.Is<CreateVehicleDto>(vehicle => vehicle.ImageUrl == "https://cdn.example.com/gla.jpg" && vehicle.ImageKey == "vehicles/gla.jpg"), It.IsAny<System.Security.Claims.ClaimsPrincipal>()))
            .ReturnsAsync(new VehicleDto { Id = 12, Brand = createVehicleDto.Brand, Model = createVehicleDto.Model, ListingType = createVehicleDto.ListingType, Status = VehicleStatus.AVAILABLE });
        var storageMock = new Mock<IStorageService>();
        storageMock.Setup(storage => storage.UploadFileAsync(imageMock.Object, "01_vehicules")).ReturnsAsync(("https://cdn.example.com/gla.jpg", "vehicles/gla.jpg"));
        var controller = new VehiclesController(repositoryMock.Object, storageMock.Object);
        TestClaimsPrincipalFactory.AttachUser(controller, TestClaimsPrincipalFactory.CreateStaff(Guid.NewGuid().ToString()));

        // Act
        var result = await controller.AddVehicle(createVehicleDto, imageMock.Object);

        // Assert
        Assert.IsType<CreatedAtActionResult>(result);
        repositoryMock.VerifyAll();
    }
    #endregion

    #region UpdateVehicle
    [Fact]
    public async Task UpdateVehicle_ShouldReturnNoContent_WhenUpdateSucceedsWithoutNewImage()
    {
        // Arrange
        var vehicle = CreateVehicleDto();
        var repositoryMock = new Mock<IVehiclesRepository>();
        repositoryMock.Setup(repository => repository.UpdateVehicleAsync(vehicle, It.IsAny<System.Security.Claims.ClaimsPrincipal>())).Returns(Task.CompletedTask);
        var storageMock = new Mock<IStorageService>();
        var controller = new VehiclesController(repositoryMock.Object, storageMock.Object);
        TestClaimsPrincipalFactory.AttachUser(controller, TestClaimsPrincipalFactory.CreateStaff(Guid.NewGuid().ToString()));

        // Act
        var result = await controller.UpdateVehicle(vehicle.Id, vehicle, null);

        // Assert
        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task UpdateVehicle_ShouldReturnBadRequest_WhenModelStateIsInvalid()
    {
        // Arrange
        var repositoryMock = new Mock<IVehiclesRepository>();
        var storageMock = new Mock<IStorageService>();
        var controller = new VehiclesController(repositoryMock.Object, storageMock.Object);
        controller.ModelState.AddModelError("Model", "required");

        // Act
        var result = await controller.UpdateVehicle(2, CreateVehicleDto(), null);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task UpdateVehicle_ShouldReturnBadRequest_WhenIdDoesNotMatchDtoId()
    {
        // Arrange
        var repositoryMock = new Mock<IVehiclesRepository>();
        var storageMock = new Mock<IStorageService>();
        var controller = new VehiclesController(repositoryMock.Object, storageMock.Object);

        // Act
        var result = await controller.UpdateVehicle(9, CreateVehicleDto(), null);

        // Assert
        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Vehicle ID mismatch", badRequest.Value);
    }

    [Fact]
    public async Task UpdateVehicle_ShouldReplaceExistingImage_WhenNewImageIsProvided()
    {
        // Arrange
        var imageMock = CreateFormFileMock("xc40-new.jpg", "image/jpeg", 1024);
        var vehicle = CreateVehicleDto();
        vehicle.ImageKey = "vehicles/xc40-old.jpg";
        var repositoryMock = new Mock<IVehiclesRepository>();
        repositoryMock.Setup(repository => repository.UpdateVehicleAsync(It.Is<VehicleDto>(updatedVehicle => updatedVehicle.ImageKey == "vehicles/xc40-new.jpg" && updatedVehicle.ImageUrl == "https://cdn.example.com/xc40-new.jpg"), It.IsAny<System.Security.Claims.ClaimsPrincipal>()))
            .Returns(Task.CompletedTask);
        var storageMock = new Mock<IStorageService>();
        storageMock.Setup(storage => storage.DeleteFileAsync("vehicles/xc40-old.jpg", "01_vehicules")).Returns(Task.CompletedTask);
        storageMock.Setup(storage => storage.UploadFileAsync(imageMock.Object, "01_vehicules")).ReturnsAsync(("https://cdn.example.com/xc40-new.jpg", "vehicles/xc40-new.jpg"));
        var controller = new VehiclesController(repositoryMock.Object, storageMock.Object);
        TestClaimsPrincipalFactory.AttachUser(controller, TestClaimsPrincipalFactory.CreateStaff(Guid.NewGuid().ToString()));

        // Act
        var result = await controller.UpdateVehicle(vehicle.Id, vehicle, imageMock.Object);

        // Assert
        Assert.IsType<NoContentResult>(result);
        storageMock.Verify(storage => storage.DeleteFileAsync("vehicles/xc40-old.jpg", "01_vehicules"), Times.Once);
        storageMock.Verify(storage => storage.UploadFileAsync(imageMock.Object, "01_vehicules"), Times.Once);
    }
    #endregion

    #region DeleteVehicle
    [Fact]
    public async Task DeleteVehicle_ShouldReturnNoContent_WhenVehicleHasNoImage()
    {
        // Arrange
        var vehicle = new VehicleDto { Id = 5, Brand = "Fiat", Model = "500", ListingType = ListingType.SALE, Status = VehicleStatus.AVAILABLE };
        var repositoryMock = new Mock<IVehiclesRepository>();
        repositoryMock.Setup(repository => repository.GetVehicleByIdAsync(5)).ReturnsAsync(vehicle);
        repositoryMock.Setup(repository => repository.DeleteVehicleAsync(5, It.IsAny<System.Security.Claims.ClaimsPrincipal>())).Returns(Task.CompletedTask);
        var storageMock = new Mock<IStorageService>();
        var controller = new VehiclesController(repositoryMock.Object, storageMock.Object);
        TestClaimsPrincipalFactory.AttachUser(controller, TestClaimsPrincipalFactory.CreateStaff(Guid.NewGuid().ToString()));

        // Act
        var result = await controller.DeleteVehicle(5);

        // Assert
        Assert.IsType<NoContentResult>(result);
        storageMock.Verify(storage => storage.DeleteFileAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task DeleteVehicle_ShouldDeleteImageBeforeDeletingVehicle_WhenImageExists()
    {
        // Arrange
        var vehicle = new VehicleDto { Id = 5, Brand = "Fiat", Model = "500", ListingType = ListingType.SALE, Status = VehicleStatus.AVAILABLE, ImageKey = "vehicles/500.jpg" };
        var repositoryMock = new Mock<IVehiclesRepository>();
        repositoryMock.Setup(repository => repository.GetVehicleByIdAsync(5)).ReturnsAsync(vehicle);
        repositoryMock.Setup(repository => repository.DeleteVehicleAsync(5, It.IsAny<System.Security.Claims.ClaimsPrincipal>())).Returns(Task.CompletedTask);
        var storageMock = new Mock<IStorageService>();
        storageMock.Setup(storage => storage.DeleteFileAsync("vehicles/500.jpg", "01_vehicules")).Returns(Task.CompletedTask);
        var controller = new VehiclesController(repositoryMock.Object, storageMock.Object);
        TestClaimsPrincipalFactory.AttachUser(controller, TestClaimsPrincipalFactory.CreateStaff(Guid.NewGuid().ToString()));

        // Act
        var result = await controller.DeleteVehicle(5);

        // Assert
        Assert.IsType<NoContentResult>(result);
        storageMock.Verify(storage => storage.DeleteFileAsync("vehicles/500.jpg", "01_vehicules"), Times.Once);
    }
    #endregion

    private static CreateVehicleDto CreateCreateVehicleDto()
    {
        return new CreateVehicleDto
        {
            Brand = "Peugeot",
            Model = "308",
            Year = 2025,
            Motorization = Motorization.Hybrid,
            Mileage = 12000,
            ListedAmount = 24990m,
            RentalTermMonths = RentalTerm.Months24,
            ListingType = ListingType.RENTAL
        };
    }

    private static VehicleDto CreateVehicleDto()
    {
        return new VehicleDto
        {
            Id = 2,
            Brand = "Peugeot",
            Model = "308",
            Year = 2025,
            Motorization = Motorization.Hybrid,
            Mileage = 12000,
            ListedAmount = 24990m,
            RentalTermMonths = RentalTerm.Months24,
            ListingType = ListingType.RENTAL,
            Status = VehicleStatus.AVAILABLE
        };
    }

    private static Mock<IFormFile> CreateFormFileMock(string fileName, string contentType, long length)
    {
        var formFileMock = new Mock<IFormFile>();
        formFileMock.Setup(file => file.FileName).Returns(fileName);
        formFileMock.Setup(file => file.ContentType).Returns(contentType);
        formFileMock.Setup(file => file.Length).Returns(length);
        return formFileMock;
    }
}