/*
    * thise file maps the application entity to the application dto
*/

using mmotors_back.Models;
using mmotors_back.Features.Applications.Dtos;

namespace mmotors_back.Mappers
{
    public static class ApplicationMapper
    {
        public static ApplicationDto ToDto(Application application) 
        {
            return new ApplicationDto
            {
                Id = application.Id,
                UserId = application.UserId,
                VehicleId = application.VehicleId,
                ApplicationType = application.ApplicationType,
                TotalAmount = application.TotalAmount,
                Status = application.Status,
                CreatedAt = application.CreatedAt,
                UpdatedAt = application.UpdatedAt,
                ApplicationServices = application.ApplicationServices.Select(s => ApplicationServiceMapper.ToDto(s)).ToList(),
                Documents = application.Documents.Select(d => DocumentMapper.ToDto(d)).ToList()
            };
        }

        public static Application ToEntity(CreateApplicationDto applicationDto) 
        {
            return new Application
            {
                UserId = applicationDto.UserId,
                VehicleId = applicationDto.VehicleId,
                ApplicationType = applicationDto.ApplicationType,
                BaseAmount = applicationDto.BaseAmount,
                TotalOverheadAmount = applicationDto.TotalOverheadAmount,
                TotalAmount = applicationDto.BaseAmount + applicationDto.TotalOverheadAmount,
                Status = ApplicationStatus.DRAFT
            };
        }
    }
}