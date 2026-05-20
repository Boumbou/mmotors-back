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
using mmotors_back.Features.Shared.Interfaces;
using Microsoft.EntityFrameworkCore;
using Xunit;
using System.Security.Claims;

namespace mmotors_back.Tests.Features.Vehicles
{
    public class VehiclesRepositoryTests
    {
        private readonly Mock<IPaginationService> _paginationServiceMock;

        public VehiclesRepositoryTests()
        {
            _paginationServiceMock = new Mock<IPaginationService>();
            _paginationServiceMock.Setup(p => p.PaginateAsync(It.IsAny<IQueryable<Vehicle>>(), It.IsAny<PaginationParams>()))
                    .ReturnsAsync((IQueryable<Vehicle> query, PaginationParams paginationParams) =>
                    {
                        var items = query.Skip((paginationParams.PageNumber - 1) * paginationParams.PageSize)
                                         .Take(paginationParams.PageSize)
                                         .ToList();
                        return new PagedResults<Vehicle>
                        {
                            Items = items,
                            TotalCount = query.Count(),
                            PageNumber = paginationParams.PageNumber,
                            PageSize = paginationParams.PageSize
                        };
                    });
        }

        #region GetAllVehiclesAsync Tests
            //test when there are vehicles and no filter is applied
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
                    new Vehicle { Brand = "BMW", Model = "Serie 3", Year = 2019, ListingType = ListingType.RENTAL, Status = VehicleStatus.RENTED },
                    new Vehicle { Brand = "Mercedes", Model = "C-Class", Year = 2021, ListingType = ListingType.SALE, Status = VehicleStatus.AVAILABLE }
                );

                await context.SaveChangesAsync();
                var repository = new VehiclesRepository(context, _paginationServiceMock.Object);

                // Act
                var result = await repository.GetAllVehiclesAsync(type: null);

                // Assert contains the expected number of vehicles
                Assert.Equal(3, result.Items.Count());
                // Assert every item contains the expected properties of the vehicles
                Assert.All(result.Items, v =>
                {
                    Assert.NotEqual(0, v.Id);
                    Assert.False(string.IsNullOrEmpty(v.Brand));
                    Assert.False(string.IsNullOrEmpty(v.Model));
                    Assert.InRange(v.Year, 1900, DateTime.Now.Year);
                    Assert.IsType<ListingType>(v.ListingType);
                    Assert.IsType<VehicleStatus>(v.Status);
                });
            }

            //test the case when there are no vehicles in the database
            [Fact]
            public async Task GetAllVehiclesAsync_ShouldReturnEmptyList_WhenNoVehiclesExist()
            {            
                // Arrange
                var options = new DbContextOptionsBuilder<AppDbContext>()
                    .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                    .Options;
                
                await using var context = new AppDbContext(options);
                var repository = new VehiclesRepository(context, _paginationServiceMock.Object);

                // Act
                var result = await repository.GetAllVehiclesAsync(type: null);

                // Assert
                Assert.Empty(result.Items);
            }

            //test the case when there are vehicle and a filter is applied
            [Fact]
            public async Task GetAllVehiclesAsync_ShouldReturnFilteredVehicles_WhenTypeFilterIsApplied()
            {
                // Arrange
                var options = new DbContextOptionsBuilder<AppDbContext>()
                    .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                    .Options;

                await using var context = new AppDbContext(options);

                context.Vehicles.AddRange(
                    new Vehicle { Brand = "Toyota", Model = "Corolla", Year = 2020, ListingType = ListingType.SALE, Status = VehicleStatus.AVAILABLE },
                    new Vehicle { Brand = "BMW", Model = "Serie 3", Year = 2019, ListingType = ListingType.RENTAL, Status = VehicleStatus.RENTED },
                    new Vehicle { Brand = "Mercedes", Model = "C-Class", Year = 2021, ListingType = ListingType.SALE, Status = VehicleStatus.AVAILABLE }
                );

                await context.SaveChangesAsync();
                var repository = new VehiclesRepository(context, _paginationServiceMock.Object);

                // Act
                var result = await repository.GetAllVehiclesAsync(type: "sale");

                // Assert contains the expected number of vehicles
                Assert.Equal(2, result.Items.Count());
                // Assert every item contains the expected properties of the vehicles
                Assert.All(result.Items, v =>
                {
                    Assert.NotEqual(0, v.Id);
                    Assert.False(string.IsNullOrEmpty(v.Brand));
                    Assert.False(string.IsNullOrEmpty(v.Model));
                    Assert.InRange(v.Year, 1900, DateTime.Now.Year);
                    Assert.Equal(ListingType.SALE, v.ListingType);
                    Assert.IsType<VehicleStatus>(v.Status);
                });
            }
        #endregion
             
        
        #region GetVehicleByIdAsync Tests
            //test get vehicle by id when the vehicle exists
            [Fact]
            public async Task GetVehicleById_ShouldReturnVehicle_WhenVehicleExists()
            {
                // Arrange
                var options = new DbContextOptionsBuilder<AppDbContext>()
                    .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                    .Options;

                await using var context = new AppDbContext(options);
                var vehicle = new Vehicle { Brand = "Toyota", Model = "Corolla", Year = 2020, ListingType = ListingType.SALE, Status = VehicleStatus.AVAILABLE };
                context.Vehicles.Add(vehicle);
                await context.SaveChangesAsync();
                var repository = new VehiclesRepository(context, _paginationServiceMock.Object);

                // Act
                var result = await repository.GetVehicleByIdAsync(vehicle.Id);

                // Assert
                Assert.NotNull(result);
                Assert.Equal(vehicle.Id, result.Id);
                Assert.False(string.IsNullOrEmpty(result.Brand));
                Assert.False(string.IsNullOrEmpty(result.Model));
                Assert.InRange(result.Year, 1900, DateTime.Now.Year);
                Assert.IsType<ListingType>(result.ListingType);
                Assert.IsType<VehicleStatus>(result.Status);
            }

            //test get vehicle by id when the vehicle does not exist
            [Fact]
            public async Task GetVehicleById_ShouldThrowKeyNotFoundException_WhenVehicleDoesNotExist()
            {
                // Arrange
                var options = new DbContextOptionsBuilder<AppDbContext>()
                    .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                    .Options;

                await using var context = new AppDbContext(options);
                var repository = new VehiclesRepository(context, _paginationServiceMock.Object);

                // Act & Assert
                await Assert.ThrowsAsync<KeyNotFoundException>(async () =>
                {
                    await repository.GetVehicleByIdAsync(999); // Assuming 999 is an ID that does not exist
                });
            }
        #endregion

        #region CreateVehicleAsync Tests
            //test create vehicle when the input is valid
            [Fact]
            public async Task CreateVehicle_ShouldAddSaleVehicle_WhenInputIsValid()
            {
                // Arrange
                var options = new DbContextOptionsBuilder<AppDbContext>()
                    .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                    .Options;

                await using var context = new AppDbContext(options);
                var repository = new VehiclesRepository(context, _paginationServiceMock.Object);
                var createDto = new CreateVehicleDto
                {
                    Brand = "Toyota",
                    Model = "Corolla",
                    Year = 2020,
                    ListingType = ListingType.SALE,
                    Motorization = Motorization.Diesel,
                    Mileage = 15000,
                    ListedAmount = 20000
                };

                //create claims
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Role, "Staff")
                };
                var identity = new ClaimsIdentity(claims, "TestAuthType");
                var user = new ClaimsPrincipal(identity);
    
                // Act
                var result = await repository.AddVehicleAsync(createDto, user);

                // Assert
                Assert.NotNull(result);
                Assert.NotEqual(0, result.Id);
                Assert.Equal(createDto.Brand, result.Brand);
                Assert.Equal(createDto.Model, result.Model);
                Assert.Equal(createDto.Year, result.Year);
                Assert.Equal(createDto.ListingType, result.ListingType);
                Assert.Equal(createDto.Motorization, result.Motorization);
                Assert.Equal(createDto.Mileage, result.Mileage);
                Assert.Equal(createDto.ListedAmount, result.ListedAmount);
                Assert.Equal(VehicleStatus.AVAILABLE, result.Status);
            }

            //test create vehicle when the input is valid for rental
            [Fact]
            public async Task CreateVehicle_ShouldAddRentalVehicle_WhenInputIsValid()
            {
                // Arrange
                var options = new DbContextOptionsBuilder<AppDbContext>()
                    .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                    .Options;
                await using var context = new AppDbContext(options);
                var repository = new VehiclesRepository(context, _paginationServiceMock.Object);
                var createDto = new CreateVehicleDto
                {
                    Brand = "BMW",
                    Model = "Serie 3",
                    Year = 2019,
                    ListingType = ListingType.RENTAL,
                    Motorization = Motorization.Petrol,
                    Mileage = 30000,
                    ListedAmount = 500,
                    RentalTermMonths = RentalTerm.Months24
                };

                //create claims
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Role, "Staff")
                };
                var identity = new ClaimsIdentity(claims, "TestAuthType");
                var user = new ClaimsPrincipal(identity);

                // Act
                var result = await repository.AddVehicleAsync(createDto, user);

                // Assert
                Assert.NotNull(result);
                Assert.NotEqual(0, result.Id);
                Assert.Equal(createDto.Brand, result.Brand);
                Assert.Equal(createDto.Model, result.Model);
                Assert.Equal(createDto.Year, result.Year);
                Assert.Equal(createDto.ListingType, result.ListingType);
                Assert.Equal(createDto.Motorization, result.Motorization);
                Assert.Equal(createDto.Mileage, result.Mileage);
                Assert.Equal(createDto.ListedAmount, result.ListedAmount);
                Assert.Equal(createDto.RentalTermMonths, result.RentalTermMonths);
                Assert.Equal(VehicleStatus.AVAILABLE, result.Status);
            }

            //test create vehicle when the input is invalid
            [Fact]
            public async Task CreateVehicle_ShouldThrowArgumentException_WhenInputIsInvalid()
            {
                // Arrange
                var options = new DbContextOptionsBuilder<AppDbContext>()
                    .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                    .Options;
                await using var context = new AppDbContext(options);
                var repository = new VehiclesRepository(context, _paginationServiceMock.Object);
                var createDto = new CreateVehicleDto
                {
                    Brand = "BMW",
                    Model = "Serie 3",
                    Year = 2019,
                    ListingType = ListingType.RENTAL,
                    Motorization = Motorization.Petrol,
                    Mileage = 30000,
                    ListedAmount = 500
                    // Missing RentalTermMonths for RENTAL listing type
                };

                //create claims
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Role, "Staff")
                };
                var identity = new ClaimsIdentity(claims, "TestAuthType");
                var user = new ClaimsPrincipal(identity);

                // Act & Assert
                await Assert.ThrowsAsync<ArgumentException>(async () =>
                {
                    await repository.AddVehicleAsync(createDto, user);
                });
            }

            //test create vehicle when the input is invalid for sale
            [Fact]
            public async Task CreateVehicle_ShouldIgnoreRentalTerm_WhenInputIsInvalidForSale()
            {
                // Arrange
                var options = new DbContextOptionsBuilder<AppDbContext>()
                    .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                    .Options;
                await using var context = new AppDbContext(options);
                var repository = new VehiclesRepository(context, _paginationServiceMock.Object);
                var createDto = new CreateVehicleDto
                {
                    Brand = "BMW",
                    Model = "Serie 3",
                    Year = 2019,
                    ListingType = ListingType.SALE,
                    Motorization = Motorization.Petrol,
                    Mileage = 30000,
                    ListedAmount = 500,
                    RentalTermMonths = RentalTerm.Months24 // RentalTermMonths should not be provided for SALE listing type
                };

                //create claims
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Role, "Staff")
                };
                var identity = new ClaimsIdentity(claims, "TestAuthType");
                var user = new ClaimsPrincipal(identity);

                // Act
                var result = await repository.AddVehicleAsync(createDto, user);

                // Assert
                Assert.NotNull(result);
                Assert.NotEqual(0, result.Id);
                Assert.Equal(createDto.Brand, result.Brand);
                Assert.Equal(createDto.Model, result.Model);
                Assert.Equal(createDto.Year, result.Year);
                Assert.Equal(createDto.ListingType, result.ListingType);
                Assert.Equal(createDto.Motorization, result.Motorization);
                Assert.Equal(createDto.Mileage, result.Mileage);
                Assert.Equal(createDto.ListedAmount, result.ListedAmount);
                Assert.Null(result.RentalTermMonths);
                Assert.Equal(VehicleStatus.AVAILABLE, result.Status);
            }
        #endregion

        #region UpdateVehicleAsync Tests
            [Fact]
            public async Task UpdateVehicleAsync_ShouldSaveChangesToDatabase_WhenVehicleExists()
            {
                // Arrange
                var options = new DbContextOptionsBuilder<AppDbContext>()
                    .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                    .Options;

                await using var context = new AppDbContext(options);
                var vehicle = new Vehicle
                {
                    Brand = "Toyota",
                    Model = "Corolla",
                    Year = 2020,
                    ListingType = ListingType.SALE,
                    Motorization = Motorization.Diesel,
                    Mileage = 15000,
                    ListedAmount = 20000,
                    Status = VehicleStatus.AVAILABLE
                };

                context.Vehicles.Add(vehicle);
                await context.SaveChangesAsync();

                var repository = new VehiclesRepository(context, _paginationServiceMock.Object);
                var updatedVehicle = new VehicleDto
                {
                    Id = vehicle.Id,
                    Brand = "BMW",
                    Model = "Serie 3",
                    Year = 2022,
                    ListingType = ListingType.RENTAL,
                    Motorization = Motorization.Hybrid,
                    Mileage = 12000,
                    ListedAmount = 650,
                    RentalTermMonths = RentalTerm.Months24,
                    Status = VehicleStatus.RENTED,
                    ImageUrl = "https://example.com/vehicle.jpg",
                    ImageKey = "vehicle-key"
                };

                //create claims
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Role, "Staff")
                };
                var identity = new ClaimsIdentity(claims, "TestAuthType");
                var user = new ClaimsPrincipal(identity);

                // Act
                await repository.UpdateVehicleAsync(updatedVehicle, user);
                var result = await repository.GetVehicleByIdAsync(vehicle.Id);

                // Assert
                Assert.NotNull(result);
                Assert.Equal(updatedVehicle.Id, result.Id);
                Assert.Equal(updatedVehicle.Brand, result.Brand);
                Assert.Equal(updatedVehicle.Model, result.Model);
                Assert.Equal(updatedVehicle.Year, result.Year);
                Assert.Equal(updatedVehicle.ListingType, result.ListingType);
                Assert.Equal(updatedVehicle.Motorization, result.Motorization);
                Assert.Equal(updatedVehicle.Mileage, result.Mileage);
                Assert.Equal(updatedVehicle.ListedAmount, result.ListedAmount);
                Assert.Equal(updatedVehicle.RentalTermMonths, result.RentalTermMonths);
                Assert.Equal(updatedVehicle.Status, result.Status);
                Assert.Equal(updatedVehicle.ImageUrl, result.ImageUrl);
                Assert.Equal(updatedVehicle.ImageKey, result.ImageKey);
            }

            [Fact]
            public async Task UpdateVehicleAsync_ShouldThrowKeyNotFoundException_WhenVehicleDoesNotExist()
            {
                // Arrange
                var options = new DbContextOptionsBuilder<AppDbContext>()
                    .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                    .Options;

                await using var context = new AppDbContext(options);
                var repository = new VehiclesRepository(context, _paginationServiceMock.Object);
                var vehicle = new VehicleDto
                {
                    Id = 999,
                    Brand = "BMW",
                    Model = "Serie 3",
                    Year = 2022,
                    ListingType = ListingType.SALE,
                    Motorization = Motorization.Hybrid,
                    Mileage = 12000,
                    ListedAmount = 25000,
                    Status = VehicleStatus.AVAILABLE
                };

                //create claims
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Role, "Staff")
                };
                var identity = new ClaimsIdentity(claims, "TestAuthType");
                var user = new ClaimsPrincipal(identity);

                // Act & Assert
                await Assert.ThrowsAsync<KeyNotFoundException>(async () =>
                {
                    await repository.UpdateVehicleAsync(vehicle, user);
                });
            }
        #endregion

        #region DeleteVehicleAsync Tests
            [Fact]
            public async Task DeleteVehicleAsync_ShouldRemoveVehicleFromDatabase_WhenVehicleExists()
            {
                // Arrange
                var options = new DbContextOptionsBuilder<AppDbContext>()
                    .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                    .Options;

                await using var context = new AppDbContext(options);
                var vehicle = new Vehicle
                {
                    Brand = "Toyota",
                    Model = "Corolla",
                    Year = 2020,
                    ListingType = ListingType.SALE,
                    Motorization = Motorization.Diesel,
                    Mileage = 15000,
                    ListedAmount = 20000,
                    Status = VehicleStatus.AVAILABLE
                };

                context.Vehicles.Add(vehicle);
                await context.SaveChangesAsync();

                var repository = new VehiclesRepository(context, _paginationServiceMock.Object);

                //create claims
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Role, "Staff")
                };
                var identity = new ClaimsIdentity(claims, "TestAuthType");
                var user = new ClaimsPrincipal(identity);

                // Act
                await repository.DeleteVehicleAsync(vehicle.Id, user);

                // Assert
                await Assert.ThrowsAsync<KeyNotFoundException>(async () =>
                {
                    await repository.GetVehicleByIdAsync(vehicle.Id);
                });
            }

            [Fact]
            public async Task DeleteVehicleAsync_ShouldThrowKeyNotFoundException_WhenVehicleDoesNotExist()
            {
                // Arrange
                var options = new DbContextOptionsBuilder<AppDbContext>()
                    .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                    .Options;

                await using var context = new AppDbContext(options);
                var repository = new VehiclesRepository(context, _paginationServiceMock.Object);

                // Act & Assert
                await Assert.ThrowsAsync<KeyNotFoundException>(async () =>
                {
                    //create claims
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Role, "Staff")
                    };
                    var identity = new ClaimsIdentity(claims, "TestAuthType");
                    var user = new ClaimsPrincipal(identity);

                    await repository.DeleteVehicleAsync(999, user);
                });
            }

            //test delete throw exception if vehicle has ongoing applications
            [Fact]
            public async Task DeleteVehicleAsync_ShouldThrowInvalidOperationException_WhenVehicleHasOngoingApplications()
            {
                // Arrange
                var options = new DbContextOptionsBuilder<AppDbContext>()
                    .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                    .Options;
                await using var context = new AppDbContext(options);
                var vehicle = new Vehicle
                {
                    Brand = "Toyota",
                    Model = "Corolla",
                    Year = 2020,
                    ListingType = ListingType.SALE,
                    Motorization = Motorization.Diesel,
                    Mileage = 15000,
                    ListedAmount = 20000,
                    Status = VehicleStatus.AVAILABLE,
                    Applications = new List<Application>
                    {
                        new Application {UserId = "1", Status = ApplicationStatus.SUBMITTED },
                        new Application {UserId = "2", Status = ApplicationStatus.ON_HOLD }
                    }
                };

                context.Vehicles.Add(vehicle);
                await context.SaveChangesAsync();
                var repository = new VehiclesRepository(context, _paginationServiceMock.Object);

                //create claims
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Role, "Staff")
                };
                var identity = new ClaimsIdentity(claims, "TestAuthType");
                var user = new ClaimsPrincipal(identity);

                // Act & Assert
                await Assert.ThrowsAsync<InvalidOperationException>(async () =>
                {                    
                    await repository.DeleteVehicleAsync(vehicle.Id, user);
                });
            }
        #endregion
    
    }
}
