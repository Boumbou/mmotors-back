/*
    * this file maps the application entity to the application dto
*/

using mmotors_back.Models;
using mmotors_back.Features.Applications.Dtos;

namespace mmotors_back.Mappers
{
    public static class ApplicationServiceMapper
    {
        public static ApplicationServiceDto ToDto(ApplicationService Service) 
        {
            return new ApplicationServiceDto
            {
                ServiceId = Service.ServiceId,
                AppliedOverheadType = Service.AppliedOverheadType,
                AppliedOverheadValue = Service.AppliedOverheadValue,
                CalculatedOverheadAmount = Service.CalculatedOverheadAmount
            };
        }
    }
}