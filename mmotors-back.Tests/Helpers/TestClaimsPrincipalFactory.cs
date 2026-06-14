using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace mmotors_back.Tests.Helpers;

public static class TestClaimsPrincipalFactory
{
    public static ClaimsPrincipal CreateCustomer(string userId)
    {
        return CreateUser(userId, "Customer");
    }

    public static ClaimsPrincipal CreateStaff(string userId)
    {
        return CreateUser(userId, "Staff");
    }

    public static ClaimsPrincipal CreateAdmin(string userId)
    {
        return CreateUser(userId, "Admin");
    }

    public static void AttachUser(ControllerBase controller, ClaimsPrincipal user)
    {
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = user
            }
        };
    }

    private static ClaimsPrincipal CreateUser(string userId, string role)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId),
            new(ClaimTypes.Name, userId),
            new(ClaimTypes.Role, role)
        };

        return new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuthType"));
    }
}