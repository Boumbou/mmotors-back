/*
    * this files tests the application repository
    * it tests the following methods:
    * - CreateApplicationAsync
    * - GetApplicationByIdAsync
    * - GetAllApplicationsAsync
    * - GetApplicationsByUserIdAsync
    * - GetApplicationByVehicleIdAsync
    * - UpdateApplicationAsync
    * - SubmitApplicationAsync
    * - HoldApplicationAsync
    * - ReviewApplicationAsync
*/

using mmotors_back.Features.Applications.Interfaces;
using Moq;
using mmotors_back.Features.Applications.Dtos;
using mmotors_back.Models;
using mmotors_back.Data;
using mmotors_back.Features.Applications.Repositories;
using Microsoft.EntityFrameworkCore;
using Xunit;
using mmotors_back.Features.Shared.Interfaces;


namespace mmotors_back.Tests.Features.Applications
{
    public class ApplicationRepositoryTests
    {
        private readonly Mock<IPaginationService> _paginationServiceMock;
        public ApplicationRepositoryTests()
        {
            _paginationServiceMock = new Mock<IPaginationService>();
            _paginationServiceMock.Setup(p => p.PaginateAsync(It.IsAny<IQueryable<Application>>(), It.IsAny<PaginationParams>()))
                    .ReturnsAsync((IQueryable<Application> query, PaginationParams paginationParams) =>
                    {
                        var items = query.Skip((paginationParams.PageNumber - 1) * paginationParams.PageSize)
                                         .Take(paginationParams.PageSize)
                                         .ToList();
                        return new PagedResults<Application>
                        {
                            Items = items,
                            TotalCount = query.Count(),
                            PageNumber = paginationParams.PageNumber,
                            PageSize = paginationParams.PageSize
                        };
                    });
        }

        #region CreateApplicationAsync tests
            //test create application when the application is valid
            [Fact]
            public async Task CreateApplicationAsync_ShouldCreateApplication_WhenApplicationIsValid()
            {
                // Arrange
                var options = new DbContextOptionsBuilder<AppDbContext>()
                    .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                    .Options;

                await using var context = new AppDbContext(options);

                context.Vehicles.Add(new Vehicle { Id = 1, Brand = "Toyota", Model = "Corolla", Year = 2020, ListingType = ListingType.SALE, Status = VehicleStatus.AVAILABLE });
                await context.SaveChangesAsync();

                var repository = new ApplicationRepository(context, null!);

                var application = new CreateApplicationDto
                {
                    UserId = Guid.NewGuid().ToString(),
                    VehicleId = 1,
                };

                // Act
                var result = await repository.CreateApplicationAsync(application);

                // Assert
                Assert.NotNull(result);
                Assert.Equal(application.UserId, result.UserId);
                Assert.Equal(application.VehicleId, result.VehicleId);
                Assert.Equal(ApplicationStatus.DRAFT, result.Status);
            }

            //test create application returns all the expected properties
            [Fact]
            public async Task CreateApplicationAsync_ShouldReturnApplicationWithAllProperties_WhenApplicationIsValid()
            {
                // Arrange
                var options = new DbContextOptionsBuilder<AppDbContext>()
                    .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                    .Options;

                await using var context = new AppDbContext(options);

                context.Vehicles.Add(new Vehicle { Id = 1, Brand = "Toyota", Model = "Corolla", Year = 2020, ListingType = ListingType.SALE, Status = VehicleStatus.AVAILABLE });
                context.Services.Add(new Service { Id = 1, Name = "Service 1", Description = "Service 1 Description", OverheadType = OverheadType.FIXED_AMOUNT, OverheadValue = 500 });
                context.DocumentTemplates.Add(new DocumentTemplate { Id = 1, Name = "Document 1", Type = DocumentType.COMMON_APPLICATION, IsActive = true });
                
                await context.SaveChangesAsync();

                var repository = new ApplicationRepository(context, null!);

                var application = new CreateApplicationDto
                {
                    UserId = Guid.NewGuid().ToString(),
                    VehicleId = 1,
                    ApplicationType = ListingType.RENTAL,
                    BaseAmount = 20000,
                    ServiceIds = new List<int> { 1},
                };

                // Act
                var result = await repository.CreateApplicationAsync(application);

                // Assert
                Assert.NotNull(result);
                //assert all the expected properties

                
                Assert.Equal(application.UserId, result.UserId);
                Assert.Equal(application.VehicleId, result.VehicleId);
                Assert.Equal(application.ApplicationType, result.ApplicationType);
                Assert.Equal(application.BaseAmount + result.ApplicationServices.Sum(s => s.CalculatedOverheadAmount), result.TotalAmount);
                Assert.NotEqual(default(DateTime), result.CreatedAt);
                Assert.NotEqual(default(DateTime), result.UpdatedAt);
                Assert.Equal(ApplicationStatus.DRAFT, result.Status);
                Assert.NotEmpty(result.Documents);
                Assert.NotEmpty(result.ApplicationServices);
             
            }

            //test create application when the vehicle is rented or sold
            [Fact]
            public async Task CreateApplicationAsync_ShouldThrowException_WhenVehicleIsNotAvailable()
            {
                // Arrange
                var options = new DbContextOptionsBuilder<AppDbContext>()
                    .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                    .Options;

                await using var context = new AppDbContext(options);

                context.Vehicles.Add(new Vehicle { Id = 1, Brand = "Toyota", Model = "Corolla", Year = 2020, ListingType = ListingType.SALE, Status = VehicleStatus.SOLD });
                await context.SaveChangesAsync();

                var repository = new ApplicationRepository(context,_paginationServiceMock.Object);

                var application = new CreateApplicationDto
                {
                    UserId = Guid.NewGuid().ToString(),
                    VehicleId = 1,
                };

                // Act & Assert
                await Assert.ThrowsAsync<InvalidOperationException>(() => repository.CreateApplicationAsync(application));
            }
        #endregion
    
        #region GetApplicationByIdAsync tests
            //test get application by id when the application exists
            [Fact]
            public async Task GetApplicationByIdAsync_ShouldReturnApplication_WhenApplicationExists()
            {
                // Arrange
                var options = new DbContextOptionsBuilder<AppDbContext>()
                    .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                    .Options;

                await using var context = new AppDbContext(options);
                var application = new Application { UserId = Guid.NewGuid().ToString(), VehicleId = 1, Status = ApplicationStatus.DRAFT };
                context.Applications.Add(application);
                await context.SaveChangesAsync();
                var repository = new ApplicationRepository(context,_paginationServiceMock.Object);

                // Act
                var result = await repository.GetApplicationByIdAsync(application.Id);

                // Assert
                Assert.NotNull(result);
                Assert.Equal(application.Id, result.Id);
                Assert.Equal(application.UserId, result.UserId);
                Assert.Equal(application.VehicleId, result.VehicleId);
                Assert.Equal(application.Status, result.Status);
            }

            //test get application by id when the application does not exist
            [Fact]
            public async Task GetApplicationByIdAsync_ShouldThrowKeyNotFoundException_WhenApplicationDoesNotExist()
            {
                // Arrange
                var options = new DbContextOptionsBuilder<AppDbContext>()
                    .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                    .Options;

                await using var context = new AppDbContext(options);
                var repository = new ApplicationRepository(context, _paginationServiceMock.Object);

                // Act & Assert
                await Assert.ThrowsAsync<KeyNotFoundException>(async () =>
                {
                    await repository.GetApplicationByIdAsync(999); // Assuming 999 is an ID that does not exist
                });
            }
        #endregion
    
        #region GetAllApplicationsAsync tests
            //test get all applications when there are applications in the database
            [Fact]
            public async Task GetAllApplicationsAsync_ShouldReturnAllApplications_WhenApplicationsExist()
            {
                // Arrange
                var options = new DbContextOptionsBuilder<AppDbContext>()
                    .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                    .Options;

                await using var context = new AppDbContext(options);


                context.Applications.Add(new Application { UserId = Guid.NewGuid().ToString(), VehicleId = 1, Status = ApplicationStatus.DRAFT });
                context.Applications.Add(new Application { UserId = Guid.NewGuid().ToString(), VehicleId = 2, Status = ApplicationStatus.DRAFT });
                await context.SaveChangesAsync();

                
                var repository = new ApplicationRepository(context,_paginationServiceMock.Object);

                // Act
                PagedResults<ApplicationDto> result = await repository.GetAllApplicationsAsync(new PaginationParams { PageNumber = 1, PageSize = 10 });

                // Assert
                Assert.NotNull(result);
                Assert.Equal(2, result.Items.Count());
            }

            //test get all applications when there are no applications in the database
            [Fact]
            public async Task GetAllApplicationsAsync_ShouldReturnEmptyList_WhenNoApplicationsExist()
            {
                // Arrange
                var options = new DbContextOptionsBuilder<AppDbContext>()
                    .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                    .Options;

                await using var context = new AppDbContext(options);
                var repository = new ApplicationRepository(context,_paginationServiceMock.Object);

                // Act
                PagedResults<ApplicationDto> result = await repository.GetAllApplicationsAsync(new PaginationParams { PageNumber = 1, PageSize = 10 });

                // Assert
                Assert.NotNull(result);
                Assert.Empty(result.Items);
            }
        #endregion
    
        #region DeleteApplicationAsync tests
            //test delete application when the application exists
            [Fact]
            public async Task DeleteApplicationAsync_ShouldDeleteApplication_WhenApplicationExists()
            {
                // Arrange
                var options = new DbContextOptionsBuilder<AppDbContext>()
                    .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                    .Options;

                await using var context = new AppDbContext(options);
                var application = new Application { UserId = Guid.NewGuid().ToString(), VehicleId = 1, Status = ApplicationStatus.DRAFT };
                context.Applications.Add(application);
                await context.SaveChangesAsync();
                var repository = new ApplicationRepository(context, _paginationServiceMock.Object);

                // Act
                await repository.DeleteApplicationAsync(application.Id);

                // Assert
                var deletedApplication = await context.Applications.FindAsync(application.Id);
                Assert.Null(deletedApplication);
            }

            //test delete application when the application does not exist
            [Fact]
            public async Task DeleteApplicationAsync_ShouldThrowKeyNotFoundException_WhenApplicationDoesNotExist()
            {
                // Arrange
                var options = new DbContextOptionsBuilder<AppDbContext>()
                    .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                    .Options;

                await using var context = new AppDbContext(options);
                var repository = new ApplicationRepository(context, _paginationServiceMock.Object);

                // Act & Assert
                await Assert.ThrowsAsync<KeyNotFoundException>(async () =>
                {
                    await repository.DeleteApplicationAsync(999); // Assuming 999 is an ID that does not exist
                });
            }
        #endregion
    
        #region GetApplicationsByUserIdAsync tests
            //TODO: implement tests for GetApplicationsByUserIdAsync method
        #endregion

        #region GetApplicationByVehicleIdAsync tests
            //TODO: implement tests for GetApplicationByVehicleIdAsync method
        #endregion

        #region SubmitApplicationAsync tests
            //TODO: implement tests for SubmitApplicationAsync method
        #endregion

        #region HoldApplicationAsync tests
            //TODO: implement tests for HoldApplicationAsync method
        #endregion

        #region ReviewApplicationAsync tests
            //TODO: implement tests for ReviewApplicationAsync method
        #endregion

    }
}
