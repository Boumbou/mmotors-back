using mmotors_back.Features.Applications.Dtos;
using mmotors_back.Mappers;
using mmotors_back.Models;

namespace mmotors_back.Tests.Mappers;

public class ApplicationMapperTests
{
    #region ToDto
    [Fact]
    public void ToDto_ShouldMapAllFields_WhenEntityIsFullyLoaded()
    {
        // Arrange
        var application = new Application
        {
            Id = 4,
            UserId = Guid.NewGuid().ToString(),
            VehicleId = 9,
            ApplicationType = ListingType.RENTAL,
            TotalAmount = 18350m,
            Status = ApplicationStatus.SUBMITTED,
            CreatedAt = DateTime.UtcNow.AddDays(-4),
            UpdatedAt = DateTime.UtcNow.AddDays(-1),
            RejectionReason = null,
            User = new User
            {
                Id = Guid.NewGuid().ToString(),
                Email = "customer@example.com",
                UserName = "customer@example.com",
                Name = "Marie",
                LastName = "Curie"
            },
            Vehicle = new Vehicle
            {
                Id = 9,
                Brand = "Audi",
                Model = "A3",
                Year = 2025,
                ListingType = ListingType.RENTAL,
                Status = VehicleStatus.AVAILABLE
            },
            ApplicationServices = new List<ApplicationService>
            {
                new()
                {
                    ServiceId = 5,
                    AppliedOverheadType = OverheadType.FIXED_AMOUNT,
                    AppliedOverheadValue = 350m,
                    CalculatedOverheadAmount = 350m
                }
            },
            Documents = new List<Document>
            {
                new()
                {
                    Id = 2,
                    FileName = "identity-proof",
                    Type = DocumentType.COMMON_APPLICATION
                }
            }
        };

        // Act
        var result = ApplicationMapper.ToDto(application);

        // Assert
        Assert.Equal(application.Id, result.Id);
        Assert.Equal(application.UserId, result.UserId);
        Assert.Equal(application.VehicleId, result.VehicleId);
        Assert.Equal(application.ApplicationType, result.ApplicationType);
        Assert.Equal(application.TotalAmount, result.TotalAmount);
        Assert.Equal(application.Status, result.Status);
        Assert.Equal(application.CreatedAt, result.CreatedAt);
        Assert.Equal(application.UpdatedAt, result.UpdatedAt);
        Assert.NotNull(result.Vehicle);
        Assert.NotNull(result.Customer);
        Assert.Single(result.ApplicationServices);
        Assert.Single(result.Documents);
    }

    [Fact]
    public void ToDto_ShouldMapNullVehicleAndCustomer_WhenNavigationPropertiesAreMissing()
    {
        // Arrange
        var application = new Application
        {
            Id = 10,
            UserId = Guid.NewGuid().ToString(),
            VehicleId = 8,
            ApplicationType = ListingType.SALE,
            Status = ApplicationStatus.DRAFT,
            User = null!,
            Vehicle = null!
        };

        // Act
        var result = ApplicationMapper.ToDto(application);

        // Assert
        Assert.Null(result.Vehicle);
        Assert.Null(result.Customer);
    }

    [Fact]
    public void ToDto_ShouldMapEmptyCollections_WhenNoServicesOrDocumentsExist()
    {
        // Arrange
        var application = new Application
        {
            Id = 12,
            UserId = Guid.NewGuid().ToString(),
            VehicleId = 3,
            ApplicationType = ListingType.SALE,
            Status = ApplicationStatus.DRAFT,
            ApplicationServices = new List<ApplicationService>(),
            Documents = new List<Document>(),
            User = null!,
            Vehicle = null!
        };

        // Act
        var result = ApplicationMapper.ToDto(application);

        // Assert
        Assert.NotNull(result.ApplicationServices);
        Assert.NotNull(result.Documents);
        Assert.Empty(result.ApplicationServices);
        Assert.Empty(result.Documents);
    }
    #endregion

    #region ToEntity
    [Fact]
    public void ToEntity_ShouldMapCreateDtoAndSetDraftStatus_WhenDtoIsValid()
    {
        // Arrange
        var createApplicationDto = new CreateApplicationDto
        {
            UserId = Guid.NewGuid().ToString(),
            VehicleId = 15,
            ApplicationType = ListingType.SALE,
            BaseAmount = 30000m,
            TotalOverheadAmount = 1200m
        };

        // Act
        var result = ApplicationMapper.ToEntity(createApplicationDto);

        // Assert
        Assert.Equal(createApplicationDto.UserId, result.UserId);
        Assert.Equal(createApplicationDto.VehicleId, result.VehicleId);
        Assert.Equal(createApplicationDto.ApplicationType, result.ApplicationType);
        Assert.Equal(createApplicationDto.BaseAmount, result.BaseAmount);
        Assert.Equal(createApplicationDto.TotalOverheadAmount, result.TotalOverheadAmount);
        Assert.Equal(ApplicationStatus.DRAFT, result.Status);
    }

    [Fact]
    public void ToEntity_ShouldComputeTotalAmountFromBaseAndOverhead_WhenDtoContainsOverhead()
    {
        // Arrange
        var createApplicationDto = new CreateApplicationDto
        {
            UserId = Guid.NewGuid().ToString(),
            VehicleId = 18,
            ApplicationType = ListingType.RENTAL,
            BaseAmount = 14000m,
            TotalOverheadAmount = 2100m
        };

        // Act
        var result = ApplicationMapper.ToEntity(createApplicationDto);

        // Assert
        Assert.Equal(16100m, result.TotalAmount);
    }
    #endregion
}