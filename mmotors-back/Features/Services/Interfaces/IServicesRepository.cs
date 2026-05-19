/*
    * this file define the services repository interface
    * it declares the following methods:
        * Task<IEnumerable<Service>> GetAllServicesAsync()
        * Task<Service> GetServiceByIdAsync(int id)
        * Task<Service> CreateServiceAsync(Service service)
        * Task UpdateServiceAsync(int id, Service service)
        * Task DeleteServiceAsync(int id)
        * Task ToggleServiceStatusAsync(int id)
 */

using mmotors_back.Features.Services.Dtos;
using mmotors_back.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace mmotors_back.Features.Services.Interfaces
{
    public interface IServicesRepository
    {
        Task<IEnumerable<ServiceDto>> GetAllServicesAsync(ListingType? listingType = null);
        Task<ServiceDto> GetServiceByIdAsync(int id);
        Task<ServiceDto> CreateServiceAsync(CreateServiceDto service);
        Task<int> UpdateServiceAsync(ServiceDto service); //return the id of the updated service, or 0 if not found
        Task<int> DeleteServiceAsync(int id); //return the id of the deleted service, or 0 if not found
        Task<int> ToggleServiceStatusAsync(int id); //return the id of the toggled service, or 0 if not found
    }
}