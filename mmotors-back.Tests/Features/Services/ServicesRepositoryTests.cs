/*
    * this file tests the services repository
    * it implements the following tests:
        * GetAllServicesAsync_ShouldReturnAllServices
        * GetAllServicesAsync_ShouldReturnEmptyList_WhenNoServices
        * GetServiceByIdAsync_ShouldReturnService_WhenServiceExists
        * GetServiceByIdAsync_ShouldReturnNull_WhenServiceDoesNotExist
        * CreateServiceAsync_ShouldAddServiceToDatabase
        * UpdateServiceAsync_ShouldSaveChangesToDatabase_WhenServiceExists
        * UpdateServiceAsync_ShouldReturnZero_WhenServiceDoesNotExist
        * DeleteServiceAsync_ShouldRemoveServiceFromDatabase_WhenServiceExists
        * DeleteServiceAsync_ShouldReturnZero_WhenServiceDoesNotExist
        * ToggleServiceStatusAsync_ShouldToggleServiceStatus_WhenServiceExists
        * ToggleServiceStatusAsync_ShouldReturnZero_WhenServiceDoesNotExist
 */

using Microsoft.EntityFrameworkCore;
using mmotors_back.Data;
using mmotors_back.Features.Services.Dtos;
using mmotors_back.Features.Services.Interfaces;
using mmotors_back.Features.Services.Repositories;
using mmotors_back.Mappers;
using mmotors_back.Models;
using Xunit;
using Xunit.Sdk;

namespace mmotors_back.Tests.Features.Services
{
    public class ServicesRepositoryTests
    {
        private readonly IServicesRepository _servicesRepository;
        private readonly AppDbContext _context;

        public ServicesRepositoryTests()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new AppDbContext(options);
            _servicesRepository = new ServicesRepository(_context);
        }
        #region getall
            //test GetAllServicesAsync_ShouldReturnAllServices
            [Fact]
            public async Task GetAllServicesAsync_ShouldReturnAllServices()
            {
                // Arrange
                var service1 = new Service { Name = "Service 1", Description = "Description 1", ListingType = ListingType.SALE, OverheadType = OverheadType.PERCENTAGE, OverheadValue = 10, IsOptional = true, IsActive = true };
                var service2 = new Service { Name = "Service 2", Description = "Description 2", ListingType = ListingType.RENTAL, OverheadType = OverheadType.FIXED_AMOUNT, OverheadValue = 50, IsOptional = false, IsActive = true };
                _context.Services.AddRange(service1, service2);
                await _context.SaveChangesAsync();

                // Act
                var result = await _servicesRepository.GetAllServicesAsync();

                // Assert
                Assert.NotNull(result);
                Assert.Equal(2, result.Count());
                Assert.Contains(result, s => s.Name == "Service 1");
                Assert.Contains(result, s => s.Name == "Service 2");
            }

            //test GetAllServicesAsync_ShouldReturnRightServices_WhenListingTypeFilterIsApplied
            [Fact]
            public async Task GetAllServicesAsync_ShouldReturnRightServices_WhenListingTypeFilterIsApplied()
            {
                // Arrange
                var service1 = new Service { Name = "Service 1", Description = "Description 1", ListingType = ListingType.SALE, OverheadType = OverheadType.PERCENTAGE, OverheadValue = 10, IsOptional = true, IsActive = true };
                var service2 = new Service { Name = "Service 2", Description = "Description 2", ListingType = ListingType.RENTAL, OverheadType = OverheadType.FIXED_AMOUNT, OverheadValue = 50, IsOptional = false, IsActive = true };
                _context.Services.AddRange(service1, service2);
                await _context.SaveChangesAsync();

                // Act
                var result = await _servicesRepository.GetAllServicesAsync(ListingType.SALE);

                // Assert
                Assert.NotNull(result);
                Assert.Single(result);
                Assert.Contains(result, s => s.Name == "Service 1");
            }

            //test GetAllServicesAsync_ShouldReturnEmptyList_WhenNoServices
            [Fact]
            public async Task GetAllServicesAsync_ShouldReturnEmptyList_WhenNoServices()
            {
                // Act
                var result = await _servicesRepository.GetAllServicesAsync();

                // Assert
                Assert.NotNull(result);
                Assert.Empty(result);
            }
        #endregion

        #region getbyid
            //test GetServiceByIdAsync_ShouldReturnService_WhenServiceExists
            [Fact]
            public async Task GetServiceByIdAsync_ShouldReturnService_WhenServiceExists()
            {
                // Arrange
                var service = new CreateServiceDto { Name = "Service 1", Description = "Description 1", ListingType = ListingType.SALE, OverheadType = OverheadType.PERCENTAGE, OverheadValue = 10, IsOptional = true };
                var createdService = await _servicesRepository.CreateServiceAsync(service);                 

                // Act
                var result = await _servicesRepository.GetServiceByIdAsync(createdService.Id);

                // Assert
                Assert.NotNull(result);
                Assert.Equal(createdService.Id, result.Id);
                Assert.Equal(createdService.Name, result.Name);
                Assert.Equal(createdService.Description, result.Description);
                Assert.Equal(createdService.ListingType, result.ListingType);
                Assert.Equal(createdService.OverheadType, result.OverheadType);
                Assert.Equal(createdService.OverheadValue, result.OverheadValue);
                Assert.Equal(createdService.IsOptional, result.IsOptional);
                Assert.Equal(createdService.IsActive, result.IsActive);
            }

            //test GetServiceByIdAsync_ShouldReturnNull_WhenServiceDoesNotExist
            [Fact]
            public async Task GetServiceByIdAsync_ShouldReturnNull_WhenServiceDoesNotExist()
            {
                // Act
                var result = await _servicesRepository.GetServiceByIdAsync(999);

                // Assert
                Assert.Null(result);

            }
        #endregion

        #region create
            //test CreateServiceAsync_ShouldAddServiceToDatabase
            [Fact]
            public async Task CreateServiceAsync_ShouldAddServiceToDatabase()
            {
                // Arrange
                var service = new CreateServiceDto { Name = "Service 1", Description = "Description 1", ListingType = ListingType.SALE, OverheadType = OverheadType.PERCENTAGE, OverheadValue = 10, IsOptional = true};

                // Act
                var result = await _servicesRepository.CreateServiceAsync(service);

                // Assert
                Assert.NotNull(result);
                Assert.Equal(service.Name, result.Name);
                Assert.Equal(service.Description, result.Description);
                Assert.Equal(service.ListingType, result.ListingType);
                Assert.Equal(service.OverheadType, result.OverheadType);
                Assert.Equal(service.OverheadValue, result.OverheadValue);
                Assert.Equal(service.IsOptional, result.IsOptional);
                Assert.False( result.IsActive);    
                Assert.True(result.Id > 0);
            }
        #endregion

        #region update

            //test UpdateServiceAsync_ShouldSaveChangesToDatabase_WhenServiceExists
            [Fact]
            public async Task UpdateServiceAsync_ShouldSaveChangesToDatabase_WhenServiceExists()
            {
                // Arrange
                var service = new CreateServiceDto { Name = "Service 1", Description = "Description 1", ListingType = ListingType.SALE, OverheadType = OverheadType.PERCENTAGE, OverheadValue = 10, IsOptional = true };
                var createdService = await _servicesRepository.CreateServiceAsync(service);
                
                createdService.Name = "Updated Service 1";
                var updated = await _servicesRepository.UpdateServiceAsync(createdService);

                // Act
                var result = await _servicesRepository.GetServiceByIdAsync(createdService.Id);

                // Assert
                Assert.NotNull(result);
                Assert.Equal(1, updated);
                Assert.Equal("Updated Service 1", result.Name);
            }

            //test UpdateServiceAsync_ShouldReturnZero_WhenServiceDoesNotExist
            [Fact]
            public async Task UpdateServiceAsync_ShouldReturnZero_WhenServiceDoesNotExist()
            {
                // Arrange
                var service = new Service { Id = 999, Name = "Non-existent Service", Description = "Description", ListingType = ListingType.SALE, OverheadType = OverheadType.PERCENTAGE, OverheadValue = 10, IsOptional = true, IsActive = true };

                // Act
                var result = await _servicesRepository.UpdateServiceAsync(ServiceMapper.ToDto(service));

                // Assert
                Assert.Equal(0, result);
            }
        #endregion

        #region delete

            //test DeleteServiceAsync_ShouldRemoveServiceFromDatabase_WhenServiceExists
            [Fact]
            public async Task DeleteServiceAsync_ShouldRemoveServiceFromDatabase_WhenServiceExists()
            {
                // Arrange
                var service = new Service { Name = "Service 1", Description = "Description 1", ListingType = ListingType.SALE, OverheadType = OverheadType.PERCENTAGE, OverheadValue = 10, IsOptional = true, IsActive = true };
                _context.Services.Add(service);
                await _context.SaveChangesAsync();

                // Act
                var result = await _servicesRepository.DeleteServiceAsync(service.Id);

                // Assert
                Assert.Equal(1, result);
                var deletedService = await _servicesRepository.GetServiceByIdAsync(service.Id);
                Assert.Null(deletedService);
            }

            //test DeleteServiceAsync_ShouldReturnZero_WhenServiceDoesNotExist
            [Fact]
            public async Task DeleteServiceAsync_ShouldReturnZero_WhenServiceDoesNotExist()
            {
                // Act
                var result = await _servicesRepository.DeleteServiceAsync(999);

                // Assert
                Assert.Equal(0, result);
            }
        #endregion

        #region toggle

            //test ToggleServiceStatusAsync_ShouldToggleServiceStatus_WhenServiceExists
            [Fact]
            public async Task ToggleServiceStatusAsync_ShouldToggleServiceStatus_WhenServiceExists()
            {
                // Arrange
                var service = new CreateServiceDto { Name = "Service 1", Description = "Description 1", ListingType = ListingType.SALE, OverheadType = OverheadType.PERCENTAGE, OverheadValue = 10, IsOptional = true };
                var createdService = await _servicesRepository.CreateServiceAsync(service);

                // Act
                var result = await _servicesRepository.ToggleServiceStatusAsync(createdService.Id);

                // Assert
                Assert.Equal(1, result);
                var updatedService = await _servicesRepository.GetServiceByIdAsync(createdService.Id);
                Assert.NotNull(updatedService);
                Assert.True(updatedService.IsActive);   
            }

            //test ToggleServiceStatusAsync_ShouldReturnZero_WhenServiceDoesNotExist
            [Fact]
            public async Task ToggleServiceStatusAsync_ShouldReturnZero_WhenServiceDoesNotExist()
            {
                // Act
                var result = await _servicesRepository.ToggleServiceStatusAsync(999);

                // Assert
                Assert.Equal(0, result);
            }
        #endregion
    }
}