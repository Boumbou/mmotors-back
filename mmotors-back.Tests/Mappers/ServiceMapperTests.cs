using mmotors_back.Features.Services.Dtos;
using mmotors_back.Mappers;
using mmotors_back.Models;

namespace mmotors_back.Tests.Mappers;

public class ServiceMapperTests
{
    #region ToEntity_CreateServiceDto
    [Fact]
    public void ToEntity_ShouldMapCreateDtoFields_WhenDtoIsValid()
    {
        // Arrange
        var createServiceDto = new CreateServiceDto
        {
            Name = "Warranty",
            Description = "Extended coverage",
            ListingType = ListingType.SALE,
            OverheadType = OverheadType.FIXED_AMOUNT,
            OverheadValue = 900m,
            IsOptional = true
        };

        // Act
        var result = ServiceMapper.ToEntity(createServiceDto);

        // Assert
        Assert.Equal(createServiceDto.Name, result.Name);
        Assert.Equal(createServiceDto.Description, result.Description);
        Assert.Equal(createServiceDto.ListingType, result.ListingType);
        Assert.Equal(createServiceDto.OverheadType, result.OverheadType);
        Assert.Equal(createServiceDto.OverheadValue, result.OverheadValue);
        Assert.Equal(createServiceDto.IsOptional, result.IsOptional);
    }

    [Fact]
    public void ToEntity_ShouldPreserveFalseOptionalFlag_WhenCreateDtoIsNotOptional()
    {
        // Arrange
        var createServiceDto = new CreateServiceDto
        {
            Name = "Insurance",
            Description = "Mandatory policy",
            ListingType = ListingType.RENTAL,
            OverheadType = OverheadType.PERCENTAGE,
            OverheadValue = 0.08m,
            IsOptional = false
        };

        // Act
        var result = ServiceMapper.ToEntity(createServiceDto);

        // Assert
        Assert.False(result.IsOptional);
        Assert.False(result.IsActive);
    }
    #endregion

    #region ToDto
    [Fact]
    public void ToDto_ShouldMapAllFields_WhenEntityIsValid()
    {
        // Arrange
        var createdAt = DateTime.UtcNow.AddDays(-2);
        var updatedAt = DateTime.UtcNow;
        var service = new Service
        {
            Id = 7,
            Name = "Registration",
            Description = "Administrative steps",
            ListingType = ListingType.SALE,
            OverheadType = OverheadType.FIXED_AMOUNT,
            OverheadValue = 150m,
            IsOptional = true,
            IsActive = true,
            CreatedAt = createdAt,
            UpdatedAt = updatedAt
        };

        // Act
        var result = ServiceMapper.ToDto(service);

        // Assert
        Assert.Equal(service.Id, result.Id);
        Assert.Equal(service.Name, result.Name);
        Assert.Equal(service.Description, result.Description);
        Assert.Equal(service.ListingType, result.ListingType);
        Assert.Equal(service.OverheadType, result.OverheadType);
        Assert.Equal(service.OverheadValue, result.OverheadValue);
        Assert.Equal(service.IsOptional, result.IsOptional);
        Assert.Equal(service.IsActive, result.IsActive);
        Assert.Equal(service.CreatedAt, result.CreatedAt);
        Assert.Equal(service.UpdatedAt, result.UpdatedAt);
    }
    #endregion

    #region ToEntity_ServiceDto
    [Fact]
    public void ToEntity_ShouldMapAllFields_WhenDtoIsValid()
    {
        // Arrange
        var createdAt = DateTime.UtcNow.AddDays(-5);
        var updatedAt = DateTime.UtcNow.AddDays(-1);
        var serviceDto = new ServiceDto
        {
            Id = 9,
            Name = "Delivery",
            Description = "Home delivery",
            ListingType = ListingType.RENTAL,
            OverheadType = OverheadType.FIXED_AMOUNT,
            OverheadValue = 300m,
            IsOptional = true,
            IsActive = true,
            CreatedAt = createdAt,
            UpdatedAt = updatedAt
        };

        // Act
        var result = ServiceMapper.ToEntity(serviceDto);

        // Assert
        Assert.Equal(serviceDto.Id, result.Id);
        Assert.Equal(serviceDto.Name, result.Name);
        Assert.Equal(serviceDto.Description, result.Description);
        Assert.Equal(serviceDto.ListingType, result.ListingType);
        Assert.Equal(serviceDto.OverheadType, result.OverheadType);
        Assert.Equal(serviceDto.OverheadValue, result.OverheadValue);
        Assert.Equal(serviceDto.IsOptional, result.IsOptional);
        Assert.Equal(serviceDto.IsActive, result.IsActive);
        Assert.Equal(serviceDto.CreatedAt, result.CreatedAt);
        Assert.Equal(serviceDto.UpdatedAt, result.UpdatedAt);
    }

    [Fact]
    public void ToEntity_ShouldPreserveInactiveState_WhenServiceIsInactive()
    {
        // Arrange
        var serviceDto = new ServiceDto
        {
            Id = 10,
            Name = "Inactive service",
            Description = "Hidden from clients",
            ListingType = ListingType.SALE,
            OverheadType = OverheadType.PERCENTAGE,
            OverheadValue = 0.03m,
            IsOptional = false,
            IsActive = false,
            CreatedAt = DateTime.UtcNow.AddDays(-3),
            UpdatedAt = DateTime.UtcNow
        };

        // Act
        var result = ServiceMapper.ToEntity(serviceDto);

        // Assert
        Assert.False(result.IsActive);
        Assert.False(result.IsOptional);
    }
    #endregion
}