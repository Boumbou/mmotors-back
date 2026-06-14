using mmotors_back.Features.Vehicles.Dtos;
using mmotors_back.Mappers;
using mmotors_back.Models;

namespace mmotors_back.Tests.Mappers;

public class VehicleMapperTests
{
    #region ToDTO
    [Fact]
    public void ToDTO_ShouldMapAllFields_WhenEntityIsValid()
    {
        // Arrange
        var vehicle = new Vehicle
        {
            Id = 13,
            Brand = "Peugeot",
            Model = "208",
            Year = 2025,
            Motorization = Motorization.Hybrid,
            Mileage = 12000,
            ListedAmount = 24990m,
            RentalTermMonths = RentalTerm.Months24,
            ListingType = ListingType.RENTAL,
            Status = VehicleStatus.AVAILABLE,
            ImageUrl = "https://example.com/car.jpg",
            ImageKey = "vehicles/car.jpg"
        };

        // Act
        var result = VehicleMapper.ToDTO(vehicle);

        // Assert
        Assert.Equal(vehicle.Id, result.Id);
        Assert.Equal(vehicle.Brand, result.Brand);
        Assert.Equal(vehicle.Model, result.Model);
        Assert.Equal(vehicle.Year, result.Year);
        Assert.Equal(vehicle.Motorization, result.Motorization);
        Assert.Equal(vehicle.Mileage, result.Mileage);
        Assert.Equal(vehicle.ListedAmount, result.ListedAmount);
        Assert.Equal(vehicle.RentalTermMonths, result.RentalTermMonths);
        Assert.Equal(vehicle.ListingType, result.ListingType);
        Assert.Equal(vehicle.Status, result.Status);
        Assert.Equal(vehicle.ImageUrl, result.ImageUrl);
        Assert.Equal(vehicle.ImageKey, result.ImageKey);
    }

    [Fact]
    public void ToDTO_ShouldLeaveApplicationsNull_WhenIncludeApplicationsIsFalse()
    {
        // Arrange
        var vehicle = new Vehicle
        {
            Brand = "Renault",
            Model = "Clio",
            Year = 2023,
            ListingType = ListingType.SALE,
            Status = VehicleStatus.AVAILABLE,
            Applications = new List<Application>
            {
                new()
                {
                    UserId = Guid.NewGuid().ToString(),
                    VehicleId = 1,
                    ApplicationType = ListingType.SALE
                }
            }
        };

        // Act
        var result = VehicleMapper.ToDTO(vehicle, includeApplications: false);

        // Assert
        Assert.Null(result.Applications);
    }

    [Fact]
    public void ToDTO_ShouldMapApplications_WhenIncludeApplicationsIsTrue()
    {
        // Arrange
        var vehicle = new Vehicle
        {
            Id = 21,
            Brand = "Tesla",
            Model = "Model 3",
            Year = 2024,
            ListingType = ListingType.SALE,
            Status = VehicleStatus.AVAILABLE,
            Applications = new List<Application>
            {
                new()
                {
                    Id = 17,
                    UserId = Guid.NewGuid().ToString(),
                    VehicleId = 21,
                    ApplicationType = ListingType.SALE,
                    Status = ApplicationStatus.DRAFT
                }
            }
        };

        // Act
        var result = VehicleMapper.ToDTO(vehicle, includeApplications: true);

        // Assert
        Assert.NotNull(result.Applications);
        Assert.Single(result.Applications);
        Assert.Equal(17, result.Applications.First().Id);
    }
    #endregion

    #region ToEntity
    [Fact]
    public void ToEntity_ShouldMapAllFields_WhenCreateDtoIsValid()
    {
        // Arrange
        var createVehicleDto = new CreateVehicleDto
        {
            Brand = "BMW",
            Model = "i4",
            Year = 2026,
            Motorization = Motorization.Electric,
            Mileage = 2000,
            ListedAmount = 58990m,
            RentalTermMonths = RentalTerm.Months48,
            ListingType = ListingType.RENTAL,
            ImageUrl = "https://example.com/bmw.png",
            ImageKey = "vehicles/bmw.png"
        };

        // Act
        var result = VehicleMapper.ToEntity(createVehicleDto);

        // Assert
        Assert.Equal(createVehicleDto.Brand, result.Brand);
        Assert.Equal(createVehicleDto.Model, result.Model);
        Assert.Equal(createVehicleDto.Year, result.Year);
        Assert.Equal(createVehicleDto.Motorization, result.Motorization);
        Assert.Equal(createVehicleDto.Mileage, result.Mileage);
        Assert.Equal(createVehicleDto.ListedAmount, result.ListedAmount);
        Assert.Equal(createVehicleDto.RentalTermMonths, result.RentalTermMonths);
        Assert.Equal(createVehicleDto.ListingType, result.ListingType);
        Assert.Equal(createVehicleDto.ImageUrl, result.ImageUrl);
        Assert.Equal(createVehicleDto.ImageKey, result.ImageKey);
    }

    [Fact]
    public void ToEntity_ShouldPreserveNullImageFields_WhenImagesAreMissing()
    {
        // Arrange
        var createVehicleDto = new CreateVehicleDto
        {
            Brand = "Citroen",
            Model = "C3",
            Year = 2022,
            Motorization = Motorization.Petrol,
            Mileage = 50000,
            ListedAmount = 11990m,
            ListingType = ListingType.SALE,
            ImageUrl = null,
            ImageKey = null
        };

        // Act
        var result = VehicleMapper.ToEntity(createVehicleDto);

        // Assert
        Assert.Null(result.ImageUrl);
        Assert.Null(result.ImageKey);
    }
    #endregion
}