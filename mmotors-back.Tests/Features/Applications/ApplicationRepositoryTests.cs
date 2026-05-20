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
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;


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

                //create user and vehicle to satisfy foreign key constraints
                string userGuid = Guid.NewGuid().ToString();
                context.Users.Add(new User { Id = userGuid, UserName = "testuser" });
                await context.SaveChangesAsync();

                context.Vehicles.Add(new Vehicle { Id = 1, Brand = "Toyota", Model = "Corolla", Year = 2020, ListingType = ListingType.SALE, Status = VehicleStatus.AVAILABLE });
                await context.SaveChangesAsync();

                var repository = new ApplicationRepository(context, null!);

                var application = new CreateApplicationDto
                {
                    UserId = userGuid,
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

                //create user, vehicle, service and document template to satisfy foreign key constraints and application creation requirements
                string userGuid = Guid.NewGuid().ToString();
                context.Users.Add(new User { Id = userGuid, UserName = "testuser" });
                await context.SaveChangesAsync();

                context.Vehicles.Add(new Vehicle { Id = 1, Brand = "Toyota", Model = "Corolla", Year = 2020, ListingType = ListingType.SALE, Status = VehicleStatus.AVAILABLE });
                context.Services.Add(new Service { Id = 1, Name = "Service 1", Description = "Service 1 Description", OverheadType = OverheadType.FIXED_AMOUNT, OverheadValue = 500 });
                context.DocumentTemplates.Add(new DocumentTemplate { Id = 1, Name = "Document 1", Type = DocumentType.COMMON_APPLICATION, IsActive = true });
                
                await context.SaveChangesAsync();

                var repository = new ApplicationRepository(context, null!);

                var application = new CreateApplicationDto
                {
                    UserId = userGuid,
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

                //add vehicle and user to satisfy foreign key constraints
                context.Vehicles.Add(new Vehicle { Id = 1, Brand = "Toyota", Model = "Corolla", Year = 2020, ListingType = ListingType.SALE, Status = VehicleStatus.AVAILABLE });
                await context.SaveChangesAsync();
                string userGuid = Guid.NewGuid().ToString();
                context.Users.Add(new User { Id = userGuid, UserName = "testuser" });
                await context.SaveChangesAsync();

                var application = new Application { UserId = userGuid, VehicleId = 1, Status = ApplicationStatus.DRAFT };
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
        
            //test get application by id when the the user does not have access to the application
            [Fact]
            public async Task GetApplicationByIdAsync_ShouldThrowUnauthorizedAccessException_WhenUserDoesNotHaveAccessToApplication()
            {
                // Arrange
                var options = new DbContextOptionsBuilder<AppDbContext>()
                    .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                    .Options;

                await using var context = new AppDbContext(options);

                //add vehicle and user to satisfy foreign key constraints
                context.Vehicles.Add(new Vehicle { Id = 1, Brand = "Toyota", Model = "Corolla", Year = 2020, ListingType = ListingType.SALE, Status = VehicleStatus.AVAILABLE });
                await context.SaveChangesAsync();

                string userGuid = Guid.NewGuid().ToString();
                context.Users.Add(new User { Id = userGuid, UserName = "testuser" });
                await context.SaveChangesAsync();

                //create claim for testuser
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, Guid.NewGuid().ToString()),
                    new Claim(ClaimTypes.Role, "Customer")
                };
                var identity = new ClaimsIdentity(claims, "TestAuthType");
                var userClaims = new ClaimsPrincipal(identity);

                var application = new Application { UserId = userGuid, VehicleId = 1, Status = ApplicationStatus.DRAFT };
                context.Applications.Add(application);
                await context.SaveChangesAsync();
                var repository = new ApplicationRepository(context, _paginationServiceMock.Object);

                // Act & Assert
                await Assert.ThrowsAsync<UnauthorizedAccessException>(async () =>
                {
                    // Assuming the method checks for user access and the current user does not have access to the application
                    await repository.GetApplicationByIdAsync(application.Id, userClaims);
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

                //add vehicle and user to satisfy foreign key constraints
                context.Vehicles.Add(new Vehicle { Id = 1, Brand = "Toyota", Model = "Corolla", Year = 2020, ListingType = ListingType.SALE, Status = VehicleStatus.AVAILABLE });
                context.Vehicles.Add(new Vehicle { Id = 2, Brand = "Honda", Model = "Civic", Year = 2021, ListingType = ListingType.RENTAL, Status = VehicleStatus.AVAILABLE });
                await context.SaveChangesAsync();
                string userGuid = Guid.NewGuid().ToString();
                context.Users.Add(new User { Id = userGuid, UserName = "testuser" });
                await context.SaveChangesAsync();


                context.Applications.Add(new Application { UserId = userGuid, VehicleId = 1, Status = ApplicationStatus.DRAFT });
                context.Applications.Add(new Application { UserId = userGuid, VehicleId = 2, Status = ApplicationStatus.DRAFT });
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
                var application = new Application { UserId = Guid.NewGuid().ToString(), VehicleId = 1, Status = ApplicationStatus.SUBMITTED };
                context.Applications.Add(application);
                await context.SaveChangesAsync();
                var repository = new ApplicationRepository(context, _paginationServiceMock.Object);

                var userClaims = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
                    new Claim(ClaimTypes.Role, "Staff")
                }, "mock"));

                // Act
                await repository.DeleteApplicationAsync(application.Id, userClaims);

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
   
                var userClaims = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
                    new Claim(ClaimTypes.Role, "Staff")
                }, "mock"));

                // Act & Assert
                await Assert.ThrowsAsync<KeyNotFoundException>(async () =>
                {
                    await repository.DeleteApplicationAsync(999, userClaims); // Assuming 999 is an ID that does not exist
                });
            }

            //test a staff cannot delete application in draft status
            [Fact]
            public async Task DeleteApplicationAsync_ShouldThrowInvalidOperationException_WhenStaffTriesToDeleteDraftApplication()
            {
                // Arrange
                var options = new DbContextOptionsBuilder<AppDbContext>()
                    .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                    .Options;

                await using var context = new AppDbContext(options);
                var application = new Application { UserId = Guid.NewGuid().ToString(), VehicleId = 1, Status = ApplicationStatus.DRAFT };
                context.Applications.Add(application);
                await context.SaveChangesAsync();

                var userClaims = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
                    new Claim(ClaimTypes.Role, "Staff")
                }, "mock"));


                var repository = new ApplicationRepository(context, _paginationServiceMock.Object);

                // Act & Assert
                await Assert.ThrowsAsync<InvalidOperationException>(async () =>
                {
                    await repository.DeleteApplicationAsync(application.Id, userClaims); // Assuming staff tries to delete a draft application
                });
            }

            //test customer cannot delete application other than draft status
            [Fact]
            public async Task DeleteApplicationAsync_ShouldThrowInvalidOperationException_WhenCustomerTriesToDeleteNonDraftApplication()
            {
                // Arrange
                var options = new DbContextOptionsBuilder<AppDbContext>()
                    .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                    .Options;

                await using var context = new AppDbContext(options);
                var application = new Application { UserId = Guid.NewGuid().ToString(), VehicleId = 1, Status = ApplicationStatus.SUBMITTED };
                context.Applications.Add(application);
                await context.SaveChangesAsync();

                var userClaims = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
                    new Claim(ClaimTypes.Role, "Customer")
                }, "mock"));

                var repository = new ApplicationRepository(context, _paginationServiceMock.Object);

                // Act & Assert
                await Assert.ThrowsAsync<InvalidOperationException>(async () =>
                {
                    await repository.DeleteApplicationAsync(application.Id, userClaims); // Assuming customer tries to delete a non-draft application
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
            //test submit application when the application is in draft status
            [Fact]
            public async Task SubmitApplicationAsync_ShouldSubmitApplication_WhenApplicationIsInDraftStatus()
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
                //create claims
                var userClaims = new Claim[]
                {
                    new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
                    new Claim(ClaimTypes.Role, "Customer")
                };
                var claimsIdentity = new ClaimsIdentity(userClaims, "mock");
                var userClaimsPrincipal = new ClaimsPrincipal(claimsIdentity);

                // Act
                await repository.SubmitApplicationAsync(application.Id, userClaimsPrincipal);

                // Assert
                var updatedApplication = await context.Applications.FindAsync(application.Id);
                if(updatedApplication != null)
                {
                    Assert.Equal(ApplicationStatus.SUBMITTED, updatedApplication.Status);
                    Assert.NotNull(updatedApplication.SubmittedAt);
                }
                else
                {
                    Assert.NotNull(updatedApplication); 
                }
            }

            //test submit application when the application is in On_Hold status
            [Fact]
            public async Task SubmitApplicationAsync_ShouldSubmitApplication_WhenApplicationIsInOnHoldStatus()
            {
                // Arrange
                var options = new DbContextOptionsBuilder<AppDbContext>()
                    .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                    .Options;

                await using var context = new AppDbContext(options);
                var application = new Application { UserId = Guid.NewGuid().ToString(), VehicleId = 1, Status = ApplicationStatus.ON_HOLD };
                context.Applications.Add(application);
                await context.SaveChangesAsync();
                var repository = new ApplicationRepository(context, _paginationServiceMock.Object);

                //create claims
                var userClaims = new Claim[]
                {
                    new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
                    new Claim(ClaimTypes.Role, "Customer")
                };
                var claimsIdentity = new ClaimsIdentity(userClaims, "mock");
                var userClaimsPrincipal = new ClaimsPrincipal(claimsIdentity);

                // Act
                await repository.SubmitApplicationAsync(application.Id, userClaimsPrincipal);

                // Assert
                var updatedApplication = await context.Applications.FindAsync(application.Id);

                if (updatedApplication != null)
                {
                    Assert.Equal(ApplicationStatus.SUBMITTED, updatedApplication.Status);
                    Assert.NotNull(updatedApplication.SubmittedAt);
                }
                else
                {
                    Assert.NotNull(updatedApplication); 
                }
            }

            //test submit application when the application is not in draft or on hold status
            [Fact]
            public async Task SubmitApplicationAsync_ShouldThrowInvalidOperationException_WhenApplicationIsNotInDraftOrOnHoldStatus()
            {
                // Arrange
                var options = new DbContextOptionsBuilder<AppDbContext>()
                    .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                    .Options;

                await using var context = new AppDbContext(options);
                var application = new Application { UserId = Guid.NewGuid().ToString(), VehicleId = 1, Status = ApplicationStatus.SUBMITTED };
                context.Applications.Add(application);
                await context.SaveChangesAsync();
                var repository = new ApplicationRepository(context, _paginationServiceMock.Object);

                //create claims
                var userClaims = new Claim[]
                {
                    new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
                    new Claim(ClaimTypes.Role, "Customer")
                };
                var claimsIdentity = new ClaimsIdentity(userClaims, "mock");
                var userClaimsPrincipal = new ClaimsPrincipal(claimsIdentity);

                // Act & Assert
                await Assert.ThrowsAsync<InvalidOperationException>(() => repository.SubmitApplicationAsync(application.Id, userClaimsPrincipal));
            }
        #endregion

        #region HoldApplicationAsync tests
            //TODO: implement tests for HoldApplicationAsync method
            //test hold application when the application is in submitted status
            [Fact]
            public async Task HoldApplicationAsync_ShouldHoldApplication_WhenApplicationIsInSubmittedStatus()
            {
                // Arrange
                var options = new DbContextOptionsBuilder<AppDbContext>()
                    .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                    .Options;

                await using var context = new AppDbContext(options);

                var application = new Application { UserId = Guid.NewGuid().ToString(), VehicleId = 1, Status = ApplicationStatus.SUBMITTED };
                context.Applications.Add(application);
                await context.SaveChangesAsync();
                var repository = new ApplicationRepository(context, _paginationServiceMock.Object);

                //create claims
                var userClaims = new Claim[]
                {
                    new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
                    new Claim(ClaimTypes.Role, "Staff")
                };
                var claimsIdentity = new ClaimsIdentity(userClaims, "mock");
                var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);



                // Act
                await repository.HoldApplicationAsync(application.Id, claimsPrincipal);

                // Assert
                var updatedApplication = await context.Applications.FindAsync(application.Id);
                if (updatedApplication != null)
                {
                    Assert.Equal(ApplicationStatus.ON_HOLD, updatedApplication.Status);
                }
                else
                {
                    Assert.NotNull(updatedApplication); 
                }
            }
            //test hold application when the application is not in submitted status
            [Fact]
            public async Task HoldApplicationAsync_ShouldNotHoldApplication_WhenApplicationIsNotInSubmittedStatus()
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

                //create claims
                var userClaims = new Claim[]
                {
                    new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
                    new Claim(ClaimTypes.Role, "Staff")
                };
                var claimsIdentity = new ClaimsIdentity(userClaims, "mock");
                var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);



                // Act & assert
                await Assert.ThrowsAsync<InvalidOperationException>(() => repository.HoldApplicationAsync(application.Id, claimsPrincipal));

                // Assert
                var updatedApplication = await context.Applications.FindAsync(application.Id);
                if (updatedApplication != null)
                {
                    Assert.Equal(application.Status, updatedApplication.Status);
                }
                else
                {
                    Assert.NotNull(updatedApplication); 
                }
            }

            //test hold application when the user is not staff or admin
            [Fact]
            public async Task HoldApplicationAsync_ShouldThrowUnauthorizedAccessException_WhenUserIsNotStaffOrAdmin()
            {
                // Arrange
                var options = new DbContextOptionsBuilder<AppDbContext>()
                    .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                    .Options;   

                await using var context = new AppDbContext(options);
                var application = new Application { UserId = Guid.NewGuid().ToString(), VehicleId = 1, Status = ApplicationStatus.SUBMITTED };
                context.Applications.Add(application);
                await context.SaveChangesAsync();
                var repository = new ApplicationRepository(context, _paginationServiceMock.Object); 

                //create claims
                var userClaims = new Claim[]
                {
                    new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
                    new Claim(ClaimTypes.Role, "Customer")
                };
                var claimsIdentity = new ClaimsIdentity(userClaims, "mock");
                var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

                // Act & assert

                await Assert.ThrowsAsync<UnauthorizedAccessException>(() => repository.HoldApplicationAsync(application.Id, claimsPrincipal));

                // Assert
                var updatedApplication = await context.Applications.FindAsync(application.Id);
                if (updatedApplication != null)                {
                    Assert.Equal(application.Status, updatedApplication.Status);
                }
                else                {
                    Assert.NotNull(updatedApplication); 
                }
            }


        
        #endregion

        #region ReviewApplicationAsync tests
            //TODO: implement tests for ReviewApplicationAsync method
            //test review application when the application is in submitted status and is approved
            [Fact]
            public async Task ReviewApplicationAsync_ShouldApproveApplication_WhenApplicationIsInSubmittedStatusAndIsApproved()
            {
                // Arrange
                var options = new DbContextOptionsBuilder<AppDbContext>()
                    .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                    .Options;

                await using var context = new AppDbContext(options);
                var application = new Application { UserId = Guid.NewGuid().ToString(), VehicleId = 1, Status = ApplicationStatus.SUBMITTED };
                context.Applications.Add(application);
                await context.SaveChangesAsync();
                var repository = new ApplicationRepository(context, _paginationServiceMock.Object); 

                //create claims
                var userClaims = new Claim[]
                {
                    new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
                    new Claim(ClaimTypes.Role, "Staff")
                };
                var claimsIdentity = new ClaimsIdentity(userClaims, "mock");
                var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);  

                //create review application dto
                var reviewApplicationDto = new ReviewApplicationDto
                {
                    ApplicationId = application.Id,
                    IsApproved = true,
                    RejectionReason = null
                };

                // Act
                await repository.ReviewApplicationAsync(reviewApplicationDto, claimsPrincipal);

                // Assert
                var updatedApplication = await context.Applications.FindAsync(application.Id);
                if (updatedApplication != null)                {
                    Assert.Equal(ApplicationStatus.APPROVED, updatedApplication.Status);
                    Assert.Null(updatedApplication.RejectionReason);
                }
                else {
                    Assert.NotNull(updatedApplication); 
                }
            }

            //test review application when the application is in submitted status and is rejected
            [Fact]
            public async Task ReviewApplicationAsync_ShouldRejectApplication_WhenApplicationIsInSubmittedStatusAndIsRejected()
            {
                // Arrange
                var options = new DbContextOptionsBuilder<AppDbContext>()
                    .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                    .Options;   

                await using var context = new AppDbContext(options);
                var application = new Application { UserId = Guid.NewGuid().ToString(), VehicleId = 1, Status = ApplicationStatus.SUBMITTED };
                context.Applications.Add(application);
                await context.SaveChangesAsync();
                var repository = new ApplicationRepository(context, _paginationServiceMock.Object);

                //create claims
                var userClaims = new Claim[]
                {
                    new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
                    new Claim(ClaimTypes.Role, "Staff")
                };
                var claimsIdentity = new ClaimsIdentity(userClaims, "mock");
                var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

                //create review application dto
                var reviewApplicationDto = new ReviewApplicationDto
                {
                    ApplicationId = application.Id,
                    IsApproved = false,
                    RejectionReason = "Application does not meet the requirements."
                };

                // Act
                await repository.ReviewApplicationAsync(reviewApplicationDto, claimsPrincipal);

                // Assert
                var updatedApplication = await context.Applications.FindAsync(application.Id);
                if (updatedApplication != null)                {
                    Assert.Equal(ApplicationStatus.REJECTED, updatedApplication.Status);
                    Assert.Equal(reviewApplicationDto.RejectionReason, updatedApplication.RejectionReason);
                }
                else {
                    Assert.NotNull(updatedApplication); 
                }
            }

            //test review application when the application is not in submitted status
            [Fact]
            public async Task ReviewApplicationAsync_ShouldThrowInvalidOperationException_WhenApplicationIsNotInSubmittedStatus()
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

                //create claims
                var userClaims = new Claim[]
                {
                    new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
                    new Claim(ClaimTypes.Role, "Staff")
                };
                var claimsIdentity = new ClaimsIdentity(userClaims, "mock");
                var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

                //create review application dto
                var reviewApplicationDto = new ReviewApplicationDto
                {
                    ApplicationId = application.Id,
                    IsApproved = true,
                    RejectionReason = null
                };

                // Act & Assert
                await Assert.ThrowsAsync<InvalidOperationException>(
                    () => repository.ReviewApplicationAsync(reviewApplicationDto, claimsPrincipal)
                );

                // Assert
                var updatedApplication = await context.Applications.FindAsync(application.Id);
                if (updatedApplication != null)                {
                    Assert.Equal(application.Status, updatedApplication.Status);
                    Assert.Null(updatedApplication.RejectionReason);  
                }
                else {
                    Assert.NotNull(updatedApplication); 
                }
            }   

            //test review application when the user is not staff or admin
            [Fact]
            public async Task ReviewApplicationAsync_ShouldThrowUnauthorizedAccessException_WhenUserIsNotStaffOrAdmin()
            {
                // Arrange
                var options = new DbContextOptionsBuilder<AppDbContext>()
                    .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                    .Options;
                
                await using var context = new AppDbContext(options);
                var application = new Application { UserId = Guid.NewGuid().ToString(), VehicleId = 1, Status = ApplicationStatus.SUBMITTED };
                context.Applications.Add(application);
                await context.SaveChangesAsync();
                var repository = new ApplicationRepository(context, _paginationServiceMock.Object); 

                //create claims
                var userClaims = new Claim[]
                {
                    new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
                    new Claim(ClaimTypes.Role, "Customer")
                };
                var claimsIdentity = new ClaimsIdentity(userClaims, "mock");
                var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

                //create review application dto
                var reviewApplicationDto = new ReviewApplicationDto
                {
                    ApplicationId = application.Id,
                    IsApproved = true,      
                    RejectionReason = null
                };

                // Act & Assert
                await Assert.ThrowsAsync<UnauthorizedAccessException>(
                    () => repository.ReviewApplicationAsync(reviewApplicationDto, claimsPrincipal)
                );

                // Assert
                var updatedApplication = await context.Applications.FindAsync(application.Id);
                if (updatedApplication != null)                {
                    Assert.Equal(application.Status, updatedApplication.Status);
                    Assert.Null(updatedApplication.RejectionReason);
                }
                else {
                    Assert.NotNull(updatedApplication); 
                }
            }

            //test review application reject other users applications
            [Fact]
            public async Task ReviewApplicationAsync_ShouldRejectOtherApplication_WhenApplicationIsApproved()
            {
                // Arrange
                var options = new DbContextOptionsBuilder<AppDbContext>()
                    .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                    .Options;
                
                await using var context = new AppDbContext(options);
                var application = new Application { UserId = Guid.NewGuid().ToString(), VehicleId = 1, Status = ApplicationStatus.SUBMITTED };
                var otherApplication = new Application { UserId = Guid.NewGuid().ToString(), VehicleId = 1, Status = ApplicationStatus.SUBMITTED };
                var thirdApplication = new Application { UserId = Guid.NewGuid().ToString(), VehicleId = 1, Status = ApplicationStatus.DRAFT };
                
                context.Applications.Add(application);
                context.Applications.Add(otherApplication);
                context.Applications.Add(thirdApplication);
                await context.SaveChangesAsync();
                var repository = new ApplicationRepository(context, _paginationServiceMock.Object); 

                //create claims
                var userClaims = new Claim[]
                {
                    new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
                    new Claim(ClaimTypes.Role, "Staff")
                };
                var claimsIdentity = new ClaimsIdentity(userClaims, "mock");
                var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

                //create review application dto
                var reviewApplicationDto = new ReviewApplicationDto
                {
                    ApplicationId = application.Id,
                    IsApproved = true,
                    RejectionReason = null
                };

                // Act
                await repository.ReviewApplicationAsync(reviewApplicationDto, claimsPrincipal);

                // Assert
                var updatedApplication = await context.Applications.FindAsync(application.Id);
                if (updatedApplication != null)                {
                    Assert.Equal(ApplicationStatus.APPROVED, updatedApplication.Status);
                    Assert.Null(updatedApplication.RejectionReason);

                    //other applications otherApplication and thirdApplication should be rejected
                    var updatedOtherApplications = await context.Applications.Where(a => a.VehicleId == application.VehicleId && a.Id != application.Id).ToListAsync();
                    
                    if(updatedOtherApplications.Count > 0)
                    {
                        Assert.All(
                            updatedOtherApplications, 
                            a => 
                                {
                                    Assert.Equal(ApplicationStatus.REJECTED, a.Status);
                                    Assert.NotNull(a.RejectionReason);
                                }
                        );
                    }else {
                        Assert.NotNull(updatedOtherApplications);
                    }
                }
                else {
                    Assert.NotNull(updatedApplication); 
                }
            }

        #endregion

    }
}
