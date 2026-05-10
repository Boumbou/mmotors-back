/*
    * this file tests the pagination service
    * it contains the following tests:
    * PaginateAsync_ShouldReturnPagedResults_WhenCalledWithValidParameters
    * PaginateAsync_ShouldReturnEmptyPagedResults_WhenCalledWithEmptyQuery
    * PaginateAsync_ShouldReturnPagedResultsWithCorrectPageNumberAndPageSize_WhenCalledWithValidParameters
    * PaginateAsync_ShouldReturnPagedResultsWithCorrectTotalCount_WhenCalledWithValidParameters
    * PaginateAsync_ShouldReturnPagedResultsWithCorrectTotalPages_WhenCalledWithValidParameters
*/
using mmotors_back.Features.Shared.Interfaces;
using mmotors_back.Features.Shared.Services;
using mmotors_back.Models;
using mmotors_back.Data;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;
using System.ComponentModel;

namespace mmotors_back.Tests.Services
{
    public class PaginationServiceTests
    {
        private readonly IPaginationService _paginationService;

        public PaginationServiceTests()
        {
            _paginationService = new PaginationService();
        }

        [Fact]
        public async Task PaginateAsync_ShouldReturnPagedResults_WhenCalledWithValidParameters()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: "PaginationServiceTests")
                .Options;

            await using var context = new AppDbContext(options);

            context.Vehicles.AddRange(
                new Vehicle { Brand = "Brand1", Model = "Model1", Year = 2020, Motorization = Motorization.Petrol, Mileage = 10000, ListingType = ListingType.SALE },
                new Vehicle { Brand = "Brand2", Model = "Model2", Year = 2021, Motorization = Motorization.Hybrid, Mileage = 20000, ListingType = ListingType.SALE },
                new Vehicle { Brand = "Brand3", Model = "Model3", Year = 2022, Motorization = Motorization.Diesel, Mileage = 30000, ListingType = ListingType.SALE },
                new Vehicle { Brand = "Brand4", Model = "Model4", Year = 2023, Motorization = Motorization.Petrol, Mileage = 40000, ListingType = ListingType.SALE },
                new Vehicle { Brand = "Brand5", Model = "Model5", Year = 2024, Motorization = Motorization.Electric, Mileage = 50000, ListingType = ListingType.SALE }
            );

            await context.SaveChangesAsync();
            var data = context.Vehicles.Select(v => v.Id).AsQueryable();
            var paginationParams = new PaginationParams { PageNumber = 1, PageSize = 2 };

            // Act
            var result = await _paginationService.PaginateAsync(data, paginationParams);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<PagedResults<int>>(result);
            Assert.Equal(2, result.Items.Count());
            Assert.Equal(5, result.TotalCount);
            Assert.Equal(1, result.PageNumber);
            Assert.Equal(2, result.PageSize);
            Assert.Equal(3, result.TotalPages);
        }
    }
}