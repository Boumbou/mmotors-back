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
                    CalculatedOverheadAmount =  (service.OverheadType == OverheadType.FIXED_AMOUNT) ? service.OverheadValue :
                        service.OverheadValue * newApplication.BaseAmount
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

            //get full application with related data to return as dto
            newApplication = await _context.Applications
                .Where(a => a.Id == newApplication.Id)
                .Include(a => a.ApplicationServices)
                .Include(a => a.Documents)
                .Include(a => a.User)
                .Include(a => a.Vehicle).AsSplitQuery().AsNoTracking()
                .FirstOrDefaultAsync();

            if (newApplication == null)
            {
                throw new Exception("Failed to create application.");
            }

            return ApplicationMapper.ToDto(newApplication);
        }

        public async Task<ApplicationDto> GetApplicationByIdAsync(int id, ClaimsPrincipal? userClaims = null)
        {
            
            // check if user has access to the application
            var targetApplication = await _context.Applications.FindAsync(id);
            if (targetApplication == null)            {
                throw new KeyNotFoundException($"Application with ID {id} not found.");
            }

            if (userClaims != null && userClaims.IsInRole("Customer"))
            {
                string currentUserId = userClaims.FindFirstValue(ClaimTypes.Name) ?? "";
                if (targetApplication.UserId != currentUserId)
                {
                    throw new UnauthorizedAccessException("You do not have access to this application.");
                }
            }
            
            var query = _context.Applications
                .Where(a => a.Id == id)
                .Include(a => a.ApplicationServices)
                .Include(a => a.Documents)
                .Include(a => a.User)
                .Include(a => a.Vehicle).AsSplitQuery().AsNoTracking();

            Application? application = await query.FirstOrDefaultAsync();
            if (application == null)
            {
                throw new KeyNotFoundException($"Application with ID {id} not found.");
            }

            //check if user has access to the application
            var userRole = userClaims?.FindFirstValue(ClaimTypes.Role);
            var userId = userClaims?.FindFirstValue(ClaimTypes.Name);
            if (userRole == "Customer" && application.UserId != userId)
            {
                throw new UnauthorizedAccessException("You do not have access to this application.");
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
                query = query.Where(a => a.UserId == userId);
            }
            query = query
                .Include(a => a.Vehicle)
                .Include(a => a.Documents);

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

        public async Task DeleteApplicationAsync(int applicationId, ClaimsPrincipal? userClaims = null)
        {
            var application = await _context.Applications.FindAsync(applicationId);
            if (application == null)
            {
                throw new KeyNotFoundException($"Application with ID {applicationId} not found.");
            }

            //check if application is in draft and if user is staff or admin
            var userRole = userClaims?.FindFirstValue(ClaimTypes.Role);
            if (application.Status == ApplicationStatus.DRAFT  && (userRole != "Customer"))
            {
                throw new InvalidOperationException("Seules les dossier soumis peuvent être supprimés par le personnel ou l'administrateur.");
            }

            if (application.Status != ApplicationStatus.DRAFT && userRole == "Customer")
            {
                throw new InvalidOperationException("Seules les dossiers brouillons peuvent être supprimés par le client.");
            }


            // delete application services and document first due to foreign key constraints
            var applicationServices = _context.ApplicationServices.Where(s => s.ApplicationId == applicationId);
            _context.ApplicationServices.RemoveRange(applicationServices);
            var documents = _context.Documents.Where(d => d.ApplicationId == applicationId);
            _context.Documents.RemoveRange(documents);


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

        public async Task SubmitApplicationAsync(int id, ClaimsPrincipal? userClaims = null)
        {
            //check user role
            var userRole = userClaims?.FindFirstValue(ClaimTypes.Role);
            if (userRole != "Customer")
            {
                throw new UnauthorizedAccessException("Action non autorisée.");
            }

            var application = await _context.Applications.FindAsync(id);
            if (application == null)            {
                throw new KeyNotFoundException($"Application with ID {id} not found.");
            }
            if (application.Status != ApplicationStatus.DRAFT && application.Status != ApplicationStatus.ON_HOLD)
            {
                throw new InvalidOperationException("Only applications in DRAFT or ON_HOLD status can be submitted.");
            }
            application.Status = ApplicationStatus.SUBMITTED;
            application.SubmittedAt = DateTime.UtcNow;
            _context.Applications.Update(application);
            await _context.SaveChangesAsync();
        }

        public async Task HoldApplicationAsync(int id, ClaimsPrincipal? userClaims = null)
        {
            //check user role
            var userRole = userClaims?.FindFirstValue(ClaimTypes.Role);
            if (userRole == "Customer")
            {
                throw new UnauthorizedAccessException("Action non autorisée.");
            }

            var application = await _context.Applications.FindAsync(id);
            if (application == null)
            {
                throw new KeyNotFoundException($"Impossible de mettre en attente la candidature avec l'ID {id} car elle est introuvable.");
            }

            if (application.Status != ApplicationStatus.SUBMITTED)
            {
                throw new InvalidOperationException("Seules les candidatures en statut SOUMISE peuvent être mises en attente.");
            }

            application.Status = ApplicationStatus.ON_HOLD;
            _context.Applications.Update(application);
            await _context.SaveChangesAsync();
        }

        public async Task ReviewApplicationAsync(ReviewApplicationDto reviewApplication, ClaimsPrincipal? userClaims = null)
        {
            // check user role
            var userRole = userClaims?.FindFirstValue(ClaimTypes.Role);
            if (userRole == "Customer")
            {
                throw new UnauthorizedAccessException("Action non autorisée.");
            }

            // get application
            var application = await _context.Applications.FindAsync(reviewApplication.ApplicationId);
            if (application == null)            {
                throw new KeyNotFoundException($"Le dossier {reviewApplication.ApplicationId} est introuvable.");
            }

            // validate application status
            if (application.Status != ApplicationStatus.SUBMITTED)
            {
                throw new InvalidOperationException("Seules les candidatures en statut SOUMISE peuvent être examinées.");
            }

            // update application status based on review decision
            if (reviewApplication.IsApproved)
            {
                application.Status = ApplicationStatus.APPROVED;
                application.RejectionReason = null;
            }
            else
            {
                application.Status = ApplicationStatus.REJECTED;
                application.RejectionReason = reviewApplication.RejectionReason;
            }

            _context.Applications.Update(application);
            await _context.SaveChangesAsync();

            //turn other application for the same vehicle to rejected
            var otherApplications = await _context.Applications
                .Where(a => a.VehicleId == application.VehicleId && a.Id != application.Id)
                .ToListAsync();
            if (otherApplications.Count > 0)
            {
                foreach (var otherApp in otherApplications)
                {
                    otherApp.Status = ApplicationStatus.REJECTED;
                    otherApp.RejectionReason = "Un autre dossier a été approuvé pour ce véhicule.";
                }

                _context.Applications.UpdateRange(otherApplications);
            }

            await _context.SaveChangesAsync();
        }
    }
}