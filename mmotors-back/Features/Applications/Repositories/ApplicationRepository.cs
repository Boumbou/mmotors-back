/*
    * this file implements the applications repository
    * it implements the IApplicationsRepository interface
    * its methods are:
        * Task<Application> CreateApplicationAsync(Application application)
        * Task<Application> GetApplicationByIdAsync(int id)
        * Task<IEnumerable<Application>> GetApplicationsByUserIdAsync(int userId)
        * Task UpdateApplicationAsync(Application application)
        * Task SubmitApplicationAsync(int id, int userId)
        * Task ReviewApplicationAsync(int id, int reviewerUserId, bool isApproved, string? rejectionReason)
*/

using mmotors_back.Data;
using mmotors_back.Models;
using mmotors_back.Features.Applications.Interfaces;
using mmotors_back.Features.Applications.Dtos;
using mmotors_back.Features.Shared.Interfaces;
using mmotors_back.Mappers;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Security.Claims;

namespace mmotors_back.Features.Applications.Repositories
{
    public class ApplicationRepository : IApplicationsRepository
    {
        private readonly AppDbContext _context;
        private readonly IPaginationService _paginationService;

        public ApplicationRepository(AppDbContext context, IPaginationService paginationService)
        {
            _context = context;
            _paginationService = paginationService;
        }

        public async Task<ApplicationDto> CreateApplicationAsync(CreateApplicationDto application)
        {
            var vehicle = await _context.Vehicles.FindAsync(application.VehicleId);
            if (vehicle == null)            
            {
                throw new KeyNotFoundException($"Vehicle with ID {application.VehicleId} not found.");
            }
            if (vehicle.Status != VehicleStatus.AVAILABLE)
            {
                throw new InvalidOperationException($"Vehicle with ID {application.VehicleId} is not available for application.");
            }

            var newApplication = new Application
            {
                UserId = application.UserId,
                VehicleId = application.VehicleId,
                ApplicationType = application.ApplicationType,
                BaseAmount = application.BaseAmount,
                Status = ApplicationStatus.DRAFT
            };

            //add selected services
            var services = await _context.Services.Where(s => application.ServiceIds.Contains(s.Id)).ToListAsync();
            foreach (var service in services)            {
                newApplication.ApplicationServices.Add(new ApplicationService
                {
                    ServiceId = service.Id,
                    ApplicationId = newApplication.Id,
                    AppliedOverheadType = service.OverheadType,
                    AppliedOverheadValue = service.OverheadValue,
                    CalculatedOverheadAmount = service.OverheadType switch
                    {
                        OverheadType.FIXED_AMOUNT => service.OverheadValue,
                        OverheadType.PERCENTAGE => (service.OverheadValue / 100) * newApplication.BaseAmount,
                        _ => 0
                    }
                });
            }

            //calculate total overhead amount and total amount
            newApplication.TotalOverheadAmount = newApplication.ApplicationServices.Sum(s => s.CalculatedOverheadAmount);
            newApplication.TotalAmount = newApplication.BaseAmount + newApplication.TotalOverheadAmount;


            
            //add expected documents based on listing type
            var expectedDocuments = await _context.DocumentTemplates.Where(
                d => d.IsActive && (d.Type == DocumentType.COMMON_APPLICATION ||
                     (d.Type == DocumentType.SALES_APPLICATION && vehicle.ListingType == ListingType.SALE) ||
                     (d.Type == DocumentType.RENTAL_APPLICATION && vehicle.ListingType == ListingType.RENTAL))
            ).ToListAsync();

            foreach (var docTemplate in expectedDocuments)
            {
                newApplication.Documents.Add(new Document
                {
                    FileName = docTemplate.Name,
                    Type = docTemplate.Type, // this can be used in the frontend to display the required documents for the application
                });
            }
            
            _context.Applications.Add(newApplication);
            await _context.SaveChangesAsync();
            return ApplicationMapper.ToDto(newApplication);
        }

        public async Task<ApplicationDto> GetApplicationByIdAsync(int id)
        {
            Application? application = await _context.Applications.FindAsync(id);
            if (application == null)
            {
                throw new KeyNotFoundException($"Application with ID {id} not found.");
            }
            return ApplicationMapper.ToDto(application);
        }

        public async Task<PagedResults<ApplicationDto>> GetAllApplicationsAsync(PaginationParams? paginationParams = null, ClaimsPrincipal? userClaims = null)
        {
            var query = _context.Applications.AsQueryable();

            //add filtering bbased on user if role is Customer
            if (userClaims != null && userClaims.IsInRole("Customer"))
            {
                string userId = userClaims.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";
                query = query.Where(a => a.UserId == userId).Include(a => a.ApplicationServices).Include(a => a.Documents);
            }

            var pagedApplications = await _paginationService.PaginateAsync(query, paginationParams ?? new PaginationParams { PageNumber = 1, PageSize = 10 });

            var applicationDtos = pagedApplications.Items.Select(app => ApplicationMapper.ToDto(app)).ToList();

            return new PagedResults<ApplicationDto>
            {
                Items = applicationDtos,
                TotalCount = pagedApplications.TotalCount,
                PageNumber = pagedApplications.PageNumber,
                PageSize = pagedApplications.PageSize
            };
        }

        public async Task DeleteApplicationAsync(int applicationId)
        {
            var application = await _context.Applications.FindAsync(applicationId);
            if (application == null)
            {
                throw new KeyNotFoundException($"Application with ID {applicationId} not found.");
            }

            _context.Applications.Remove(application);
            await _context.SaveChangesAsync();
        }

        public async Task<PagedResults<ApplicationDto>> GetApplicationsByUserIdAsync(string userId, PaginationParams? paginationParams = null)
        {
            //TODO: implement like GetAllApplicationsAsync but filter by userId
            throw new NotImplementedException();
        }

        public async Task<PagedResults<ApplicationDto>> GetApplicationByVehicleIdAsync(int vehicleId, PaginationParams? paginationParams = null)
        {
            //TODO: implement like GetAllApplicationsAsync but filter by vehicleId
            throw new NotImplementedException();
        }

        public async Task UpdateApplicationAsync(Application application)
        {
            throw new NotImplementedException();
        }

        public async Task SubmitApplicationAsync(int id)
        {
            //TODO: find application by id, check if it is in DRAFT or ON_HOLD status, if yes update its status to SUBMITTED and set submittedAt to now, if not throw an exception
            throw new NotImplementedException();
        }

        public async Task HoldApplicationAsync(int id)
        {
            //TODO: find application by id, check if it is in SUBMITTED status, if yes update its status to ON_HOLD, if not throw an exception
            throw new NotImplementedException();
        }

        public async Task ReviewApplicationAsync(ReviewApplicationDto reviewApplication)
        {
            //TODO: get user id from claim name
            //TODO: find application by id, check if it is in SUBMITTED status, if yes update its status to APPROVED or REJECTED based on isApproved parameter, set reviewedByUserId to reviewerUserId, set reviewedAt to now, if rejected set rejectionReason, if not throw an exception
            throw new NotImplementedException();
        }


    }
}