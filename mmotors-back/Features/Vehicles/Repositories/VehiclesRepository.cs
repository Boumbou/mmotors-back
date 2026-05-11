/*
    * this file implement the repository for vehicles
    * it will be used to interact with the database and perform CRUD operations on vehicles
    * it will be injected into the controllers to handle the business logic related to vehicles
    * it will use the AppDbContext to interact with the database
    * it will implement the IVehiclesRepository interface to ensure that it has the necessary methods for handling vehicles
    * it will use the VehicleDto to transfer data between the database and the controllers
*/

using mmotors_back.Data;
using mmotors_back.Features.Vehicles.Interfaces;
using mmotors_back.Features.Vehicles.Dtos;
using mmotors_back.Features.Shared.Interfaces;
using mmotors_back.Models;
using mmotors_back.Mappers;
using Microsoft.EntityFrameworkCore;

namespace mmotors_back.Features.Vehicles.Repositories
{
    public class VehiclesRepository : IVehiclesRepository
    {
        private readonly AppDbContext _context;
        private readonly IPaginationService _paginationService;

        public VehiclesRepository(AppDbContext context, IPaginationService paginationService)
        {
            _context = context;
            _paginationService = paginationService;
        }

        public async Task<PagedResults<VehicleDto>> GetAllVehiclesAsync(string? type = null, PaginationParams paginationParams = null)
        {
            ListingType? listingType = Enum.TryParse<ListingType>(type,true, out ListingType result) ? result : (ListingType?)null;

            var query = _context.Vehicles.AsQueryable();

            if (listingType.HasValue)
            {
                query = query.Where(v => v.ListingType == listingType.Value);
            }

            if (paginationParams != null)
            {
                var pagedResults = await _paginationService.PaginateAsync(query, paginationParams);
                return new PagedResults<VehicleDto>
                {
                    Items = pagedResults.Items.Select(v => VehicleMapper.ToDTO(v)).ToList(),
                    TotalCount = pagedResults.TotalCount,
                    PageSize = pagedResults.PageSize,
                    PageNumber = pagedResults.PageNumber,
                };
            }

            var allVehicles = await query
                .Select(v => VehicleMapper.ToDTO(v))
                .ToListAsync();

            return new PagedResults<VehicleDto>
            {
                Items = allVehicles,
                TotalCount = allVehicles.Count,
                PageSize = allVehicles.Count,
                PageNumber = 1
            };
        }

        public async Task<VehicleDto> GetVehicleByIdAsync(int id)
        {
            var vehicle = await _context.Vehicles.FindAsync(id);

            if (vehicle == null)
            {
                throw new KeyNotFoundException($"Vehicle with id {id} not found.");
            }

            return VehicleMapper.ToDTO(vehicle);
        }

        public async Task AddVehicleAsync(VehicleDto vehicle)
        {
            throw new NotImplementedException();
        }

        public async Task UpdateVehicleAsync(VehicleDto vehicle)
        {
            throw new NotImplementedException();
        }

        public async Task DeleteVehicleAsync(int id)
        {
            throw new NotImplementedException();
        }
    }
}