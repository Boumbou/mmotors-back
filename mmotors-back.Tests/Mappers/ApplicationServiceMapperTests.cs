using mmotors_back.Mappers;
using mmotors_back.Models;

namespace mmotors_back.Tests.Mappers;

public class ApplicationServiceMapperTests
{
    #region ToDto
    [Fact]
    public void ToDto_ShouldMapAllFields_WhenEntityIsValid()
    {
        // Arrange
        var applicationService = new ApplicationService
        {
            ServiceId = 12,
            AppliedOverheadType = OverheadType.FIXED_AMOUNT,
            AppliedOverheadValue = 500m,
            CalculatedOverheadAmount = 500m
        };

        // Act
        var result = ApplicationServiceMapper.ToDto(applicationService);

        // Assert
        Assert.Equal(applicationService.ServiceId, result.ServiceId);
        Assert.Equal(applicationService.AppliedOverheadType, result.AppliedOverheadType);
        Assert.Equal(applicationService.AppliedOverheadValue, result.AppliedOverheadValue);
        Assert.Equal(applicationService.CalculatedOverheadAmount, result.CalculatedOverheadAmount);
    }

    [Fact]
    public void ToDto_ShouldPreservePercentageValues_WhenOverheadTypeIsPercentage()
    {
        // Arrange
        var applicationService = new ApplicationService
        {
            ServiceId = 3,
            AppliedOverheadType = OverheadType.PERCENTAGE,
            AppliedOverheadValue = 0.12m,
            CalculatedOverheadAmount = 2400m
        };

        // Act
        var result = ApplicationServiceMapper.ToDto(applicationService);

        // Assert
        Assert.Equal(OverheadType.PERCENTAGE, result.AppliedOverheadType);
        Assert.Equal(0.12m, result.AppliedOverheadValue);
        Assert.Equal(2400m, result.CalculatedOverheadAmount);
    }
    #endregion
}