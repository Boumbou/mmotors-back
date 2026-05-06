/*
    * this file is used to test the Dataseeder class.
    * DataSeeder is used to seed iitial data like admin account
    * this file has to test the following
    * - if the seeding process is successful
    * - if the admin account is created
    * - if the admin account has the correct role
    * it require the followin mocks
    * - UserManager
    * - RoleManager
*/

using mmotors_back.Data;
using mmotors_back.Models;
using Microsoft.AspNetCore.Identity;
using Moq;

namespace mmotors_back.Tests.Helpers;

public class DataSeederTest
{
    
    private readonly Mock<UserManager<User>> _userManagerMock;
    private readonly Mock<RoleManager<IdentityRole>> _roleManagerMock;

    private readonly DataSeeder _dataSeeder;

    public DataSeederTest()
    {
        _userManagerMock = new Mock<UserManager<User>>(
            Mock.Of<IUserStore<User>>(), null!, null!, null!, null!, null!, null!, null!, null!);
        
        _roleManagerMock = new Mock<RoleManager<IdentityRole>>(
            Mock.Of<IRoleStore<IdentityRole>>(), null!, null!, null!, null!);

        _dataSeeder = new DataSeeder(_userManagerMock.Object, _roleManagerMock.Object);
    }
    
    [Fact]
    public async Task SeedData_ShouldSeedData_WhenAdminRoleExistsAndAdminUserDoesNotExist()
    {
        // Arrange
        var userManagerMock = _userManagerMock;
        var roleManagerMock = _roleManagerMock;
        User adminUser = new User
        {
            Name = "Admin",
            LastName = "User",
            UserName = "admin@example.com",
            Email = "admin@example.com"
        };

        // Setup the mocks to return expected values
        userManagerMock.Setup(um => um.CreateAsync(It.IsAny<User>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success);
        userManagerMock.Setup(um => um.AddToRoleAsync(It.IsAny<User>(), "Admin"))
            .ReturnsAsync(IdentityResult.Success);
        roleManagerMock.Setup(rm => rm.RoleExistsAsync("Admin"))
            .ReturnsAsync(true);


        // Act
        await _dataSeeder.SeedData(adminUser, "adminPassword");

        // Assert
        userManagerMock.Verify(um => um.CreateAsync(It.IsAny<User>(), It.IsAny<string>()), Times.Once);
        userManagerMock.Verify(um => um.AddToRoleAsync(It.IsAny<User>(), "Admin"), Times.Once);
    }

    // test if admin exists then nothing is done
    [Fact]
    public async Task SeedData_ShouldNotSeedData_WhenAdminRoleExistsAndAdminUserExists()
    {
        // Arrange
        var userManagerMock = _userManagerMock;
        var roleManagerMock = _roleManagerMock;
        User adminUser = new User
        {
            Name = "Admin",
            LastName = "User",
            UserName = "admin@example.com",
            Email = "admin@example.com"
        };

        // Setup
        roleManagerMock.Setup(rm => rm.RoleExistsAsync("Admin"))
            .ReturnsAsync(true);
        userManagerMock.Setup(um => um.FindByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync(adminUser);

        userManagerMock.Setup(um => um.IsInRoleAsync(It.IsAny<User>(), "Admin"))
            .ReturnsAsync(true);

        //Act
        await _dataSeeder.SeedData(adminUser, "adminPassword");

        //Assert
        userManagerMock.Verify(um => um.CreateAsync(It.IsAny<User>(), It.IsAny<string>()), Times.Never);
        userManagerMock.Verify(um => um.AddToRoleAsync(It.IsAny<User>(), "Admin"), Times.Never);
    }

    // test if role admin does not exist then throw an exception
    [Fact]
    public async Task SeedData_ShouldThrowException_WhenAdminRoleDoesNotExist()
    {
        // Arrange
        var userManagerMock = _userManagerMock;
        var roleManagerMock = _roleManagerMock;
        User adminUser = new User
        {
            Name = "Admin",
            LastName = "User",
            UserName = "admin@example.com",
            Email = "admin@example.com"
        };

        // Setup
        roleManagerMock.Setup(rm => rm.RoleExistsAsync("Admin"))
            .ReturnsAsync(false);

        //Act & Assert
        await Assert.ThrowsAsync<Exception>(() => _dataSeeder.SeedData(adminUser, "adminPassword"));

        //Asset
        userManagerMock.Verify(um => um.CreateAsync(It.IsAny<User>(), It.IsAny<string>()), Times.Never);
        userManagerMock.Verify(um => um.AddToRoleAsync(It.IsAny<User>(), "Admin"), Times.Never);
    }
}