/*
    * this file defines the application repo interface
    * it is used to define the methods that will be implemented in the application repository
    * its methods are:
        * Task<Application> CreateApplicationAsync(Application application) to create application with selected services as Application services
        * Task<Application> GetApplicationByIdAsync(int id)
        * Task<IEnumerable<Application>> GetApplicationsByUserIdAsync(int userId)
        * Task UpdateApplicationAsync(Application application)
        * Task SubmitApplicationAsync(int applicationId)
        * Task ReviewApplicationAsync(int applicationId, int reviewerUserId, bool isApproved, string? rejectionReason)
*/

using mmotors_back.Features.Applications.Dtos;
using mmotors_back.Models;
using System.Security.Claims;

namespace mmotors_back.Features.Applications.Interfaces
{
    public interface IApplicationsRepository
    {
        Task<ApplicationDto> CreateApplicationAsync(CreateApplicationDto application);
        Task<ApplicationDto> GetApplicationByIdAsync(int id, ClaimsPrincipal? userClaims = null);
        Task<PagedResults<ApplicationDto>> GetApplicationsByUserIdAsync(string userId, PaginationParams? paginationParams = null);
        Task<PagedResults<ApplicationDto>> GetAllApplicationsAsync(PaginationParams? paginationParams = null, ClaimsPrincipal? userClaims = null);
        Task<PagedResults<ApplicationDto>> GetApplicationByVehicleIdAsync(int vehicleId, PaginationParams? paginationParams = null);
        Task UpdateApplicationAsync(Application application);
        Task SubmitApplicationAsync(int applicationId, ClaimsPrincipal userClaims);
        Task HoldApplicationAsync(int applicationId, ClaimsPrincipal userClaims);
        Task ReviewApplicationAsync(ReviewApplicationDto reviewApplication, ClaimsPrincipal userClaims);
        Task DeleteApplicationAsync(int applicationId, ClaimsPrincipal userClaims);
    }
}