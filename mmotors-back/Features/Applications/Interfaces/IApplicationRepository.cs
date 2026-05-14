/*
    * this file defines the application repo interface
    * it is used to define the methods that will be implemented in the application repository
    * its methods are:
        * Task<Application> CreateApplicationAsync(Application application)
        * Task<Application> GetApplicationByIdAsync(int id)
        * Task<IEnumerable<Application>> GetApplicationsByUserIdAsync(int userId)
        * Task UpdateApplicationAsync(Application application)
        * Task SubmitApplicationAsync(int applicationId)
        * Task ReviewApplicationAsync(int applicationId, int reviewerUserId, bool isApproved, string? rejectionReason)
*/

using mmotors_back.Features.Applications.Dtos;
using mmotors_back.Models;

namespace mmotors_back.Features.Applications.Interfaces
{
    public interface IApplicationsRepository
    {
        Task<ApplicationDto> CreateApplicationAsync(CreateApplicationDto application);
        Task<ApplicationDto> GetApplicationByIdAsync(int id);
        Task<PagedResults<ApplicationDto>> GetApplicationsByUserIdAsync(string userId, PaginationParams? paginationParams = null);
        Task<PagedResults<ApplicationDto>> GetAllApplicationsAsync(PaginationParams? paginationParams = null);
        Task<PagedResults<ApplicationDto>> GetApplicationByVehicleIdAsync(int vehicleId, PaginationParams? paginationParams = null);
        Task UpdateApplicationAsync(Application application);
        Task SubmitApplicationAsync(int applicationId);
        Task HoldApplicationAsync(int applicationId);
        Task ReviewApplicationAsync(int applicationId, bool isApproved, string? rejectionReason);
        Task DeleteApplicationAsync(int applicationId);
    }
}