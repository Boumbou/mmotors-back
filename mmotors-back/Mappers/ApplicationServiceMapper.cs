/*
    * this file maps the application entity to the application dto
*/

using mmotors_back.Models;
using mmotors_back.Features.Applications.Dtos;

namespace mmotors_back.Mappers
{
    public static class ApplicationServiceMapper
    {
        public static ApplicationServiceDto ToDto(Application application) 
        {
            return new ApplicationServiceDto
            {
                ServiceId = application.ApplicationServices.FirstOrDefault()?.ServiceId ?? 0,
                AppliedOverheadType = application.ApplicationServices.FirstOrDefault()?.AppliedOverheadType ?? OverheadType.FIXED_AMOUNT,
                AppliedOverheadValue = application.ApplicationServices.FirstOrDefault()?.AppliedOverheadValue ?? 0,
                CalculatedOverheadAmount = application.ApplicationServices.FirstOrDefault()?.CalculatedOverheadAmount ?? 0
            };
        }
    }
}