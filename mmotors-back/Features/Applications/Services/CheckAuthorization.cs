
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using mmotors_back.Features.Applications.Interfaces;
using mmotors_back.Features.Applications.Dtos;

namespace mmotors_back.Features.Applications.Services
{
    public class CheckAuthorization
    {        private readonly IApplicationsRepository _applicationsRepository;

        public CheckAuthorization(IApplicationsRepository applicationsRepository)
        {
            _applicationsRepository = applicationsRepository;
        }

        public async Task<IActionResult> IsUserAuthorized(ClaimsPrincipal user, int applicationId)
        {
            ApplicationDto application;
            try
            {
                application = await _applicationsRepository.GetApplicationByIdAsync(applicationId);
            }
            catch (KeyNotFoundException)
            {
                return new NotFoundObjectResult($"Application with ID {applicationId} not found.");
            }

            string? userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null || application.UserId != userId)
            {
                return new ForbidResult();
            }

            return new OkResult();
        }
    }
}