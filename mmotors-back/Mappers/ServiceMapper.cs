/*
    * this file centralize the mapping logic between the service entity and the service dto
    * it contains the following methods:
        * ServiceDto ToDto(Service service)
        * Service ToEntity(ServiceDto serviceDto)
 */

using mmotors_back.Features.Services.Dtos;
using mmotors_back.Models;

namespace mmotors_back.Mappers
{
    public static class ServiceMapper
    {
        public static Service ToEntity(CreateServiceDto serviceDto)
        {
            return new Service
            {
                Name = serviceDto.Name,
                Description = serviceDto.Description,
                ListingType = serviceDto.ListingType,
                OverheadType = serviceDto.OverheadType,
                OverheadValue = serviceDto.OverheadValue,
                IsOptional = serviceDto.IsOptional
            };
        }

        public static ServiceDto ToDto(Service service)
        {
            return new ServiceDto
            {
                Id = service.Id,
                Name = service.Name,
                Description = service.Description,
                ListingType = service.ListingType,
                OverheadType = service.OverheadType,
                OverheadValue = service.OverheadValue,
                IsOptional = service.IsOptional,
                IsActive = service.IsActive,
                CreatedAt = service.CreatedAt,
                UpdatedAt = service.UpdatedAt
            };
        }

        public static Service ToEntity(ServiceDto serviceDto)
        {
            return new Service
            {
                Id = serviceDto.Id,
                Name = serviceDto.Name,
                Description = serviceDto.Description,
                ListingType = serviceDto.ListingType,
                OverheadType = serviceDto.OverheadType,
                OverheadValue = serviceDto.OverheadValue,
                IsOptional = serviceDto.IsOptional,
                IsActive = serviceDto.IsActive,
                CreatedAt = serviceDto.CreatedAt,
                UpdatedAt = serviceDto.UpdatedAt
            };
        }
    }
}