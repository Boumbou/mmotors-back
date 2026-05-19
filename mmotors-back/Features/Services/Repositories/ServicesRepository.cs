/*
    * this file implements the services repository based on the IServicesRepository interface
    * its methods are the following:
        * Task<IEnumerable<Service>> GetAllServicesAsync()
        * Task<Service> GetServiceByIdAsync(int id)
        * Task<Service> CreateServiceAsync(Service service)
        * Task UpdateServiceAsync(int id, Service service)
        * Task DeleteServiceAsync(int id)
        * Task ToggleServiceStatusAsync(int id)
 */

using mmotors_back.Models;
using mmotors_back.Features.Services.Interfaces;
using mmotors_back.Data;
using Microsoft.EntityFrameworkCore;
using mmotors_back.Features.Services.Dtos;
using mmotors_back.Mappers;

namespace mmotors_back.Features.Services.Repositories
{
    public class ServicesRepository : IServicesRepository
    {
        private readonly AppDbContext _context;

        public ServicesRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ServiceDto>> GetAllServicesAsync(ListingType? listingType = null)
        {
            var query = _context.Services.AsQueryable();
            if (listingType.HasValue)
            {
                query = query.Where(s => s.ListingType == listingType.Value);
            }
            var services = await query.ToListAsync();
            return services.Select(ServiceMapper.ToDto);
        }

        public async Task<ServiceDto> GetServiceByIdAsync(int id)
        {
            var service = await _context.Services.FindAsync(id);
            return service == null ? null : ServiceMapper.ToDto(service);
        }

        public async Task<ServiceDto> CreateServiceAsync(CreateServiceDto service)
        {
            var createdService = await _context.Services.AddAsync(ServiceMapper.ToEntity(service));
            if(await _context.SaveChangesAsync() == 0)
            {
                throw new Exception("Failed to create service");
            }
            
            return ServiceMapper.ToDto(createdService.Entity);
        }

        public async Task<int> UpdateServiceAsync(ServiceDto service)
        {
            var existingService = await _context.Services.FindAsync(service.Id);
            if (existingService == null)
            {
                return 0;
            }

            existingService.Name = service.Name;
            existingService.Description = service.Description;
            existingService.ListingType = service.ListingType;
            existingService.OverheadType = service.OverheadType;
            existingService.OverheadValue = service.OverheadValue;
            existingService.IsOptional = service.IsOptional;
            existingService.IsActive = service.IsActive;

            // Update the service in the database
            _context.Services.Update(existingService);
            await _context.SaveChangesAsync();
            return service.Id;
        }

        public async Task<int> DeleteServiceAsync(int id)
        {
            var service = await _context.Services.FindAsync(id);
            if (service == null)
            {
                return 0;
            }

            _context.Services.Remove(service);
            await _context.SaveChangesAsync();
            return id;
        }

        public async Task<int> ToggleServiceStatusAsync(int id)
         {
             var service = await _context.Services.FindAsync(id);
             if (service == null)
              {
                  return 0;
              }

             service.IsActive = !service.IsActive;
             await _context.SaveChangesAsync();

             return id;
         }  
    }
}