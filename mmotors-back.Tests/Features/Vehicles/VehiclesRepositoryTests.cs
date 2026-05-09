/*
    *this file test implementation of IVehiclesRepository
    *it uses in-memory database to test the repository methods
    *it uses xUnit as the testing framework
    *it uses Moq to mock the database context
*/
using mmotors_back.Features.Vehicles.Interfaces;
using Moq;
using mmotors_back.Features.Vehicles.Dtos;
using mmotors_back.Models;
using mmotors_back.Data;
using mmotors_back.Features.Vehicles.Repositories;
using Microsoft.EntityFrameworkCore;
using Xunit;
using System.Reflection;

namespace mmotors_back.Tests.Features.Vehicles
{
    public class VehiclesRepositoryTests
    {
        private readonly Mock<AppDbContext> _context;

        [Fact]
        public async Task GetAllVehiclesAsync_ShouldReturnAllVehicles_WhenVehiclesExist()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            await using var context = new AppDbContext(options);

            context.Vehicles.AddRange(
                new Vehicle { Brand = "Toyota", Model = "Corolla", Year = 2020, ListingType = ListingType.SALE, Status = VehicleStatus.AVAILABLE },
                new Vehicle { Brand = "BMW", Model = "Serie 3", Year = 2019, ListingType = ListingType.RENTAL, Status = VehicleStatus.RENTED }
            );

            await context.SaveChangesAsync();
            var repository = new VehiclesRepository(context);

            // Act
            var result = await repository.GetAllVehiclesAsync();

            // Assert contains the expected number of vehicles
            Assert.Equal(2, result.Count());
            // Assert every item contains the expected properties of the vehicles
            Assert.All(result, v =>
            {
                Assert.NotEqual(0, v.Id);
                Assert.False(string.IsNullOrEmpty(v.Brand));
                Assert.False(string.IsNullOrEmpty(v.Model));
                Assert.InRange(v.Year, 1900, DateTime.Now.Year);
                Assert.IsType<ListingType>(v.ListingType);
                Assert.IsType<VehicleStatus>(v.Status);
            });
        }

        [Fact]
        public async Task GetAllVehiclesAsync_ShouldReturnEmptyList_WhenNoVehiclesExist()
        {            
            // Arrange
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            
            await using var context = new AppDbContext(options);
            var repository = new VehiclesRepository(context);

            // Act
            var result = await repository.GetAllVehiclesAsync();

            // Assert
            Assert.Empty(result);
        }
   
        
   
    }
}