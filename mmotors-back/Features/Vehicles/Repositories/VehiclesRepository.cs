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
using System.Security.Claims;

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

        public async Task<PagedResults<VehicleDto>> GetAllVehiclesAsync(string? type = null, PaginationParams? paginationParams = null)
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
            var vehicle = await _context.Vehicles
                .Include(v => v.Applications)
                .FirstOrDefaultAsync(v => v.Id == id);

            if (vehicle == null)
            {
                throw new KeyNotFoundException($"Vehicle with id {id} not found.");
            }

            return VehicleMapper.ToDTO(vehicle, includeApplications: true);
        }

        public async Task<VehicleDto> AddVehicleAsync(CreateVehicleDto vehicle, ClaimsPrincipal user)
        {
            //check role
            if(user.IsInRole("Customer"))
            {
                throw new UnauthorizedAccessException("Action non autorisée pour les clients.");
            }

            if(vehicle.ListingType == ListingType.RENTAL && !vehicle.RentalTermMonths.HasValue)
            {
                throw new ArgumentException("La durée de location doit être spécifiée pour les véhicules en location.");
            }

            if(vehicle.ListingType == ListingType.SALE)
            {
                vehicle.RentalTermMonths = null; // Ensure rental term is null for sale listings
            }

            var newVehicle = VehicleMapper.ToEntity(vehicle);
            _context.Vehicles.Add(newVehicle);
            await _context.SaveChangesAsync();
            return VehicleMapper.ToDTO(newVehicle);
        }

        public async Task UpdateVehicleAsync(VehicleDto vehicle, ClaimsPrincipal user)
        {
            //check role
            if(user.IsInRole("Customer"))
            {
                throw new UnauthorizedAccessException("Action non autorisée pour les clients.");
            }

            var existingVehicle = await _context.Vehicles.FindAsync(vehicle.Id);

            if (existingVehicle == null)
            {
                throw new KeyNotFoundException($"Vehicle with id {vehicle.Id} not found.");
            }

            existingVehicle.Brand = vehicle.Brand;
            existingVehicle.Model = vehicle.Model;
            existingVehicle.Year = vehicle.Year;
            existingVehicle.Motorization = vehicle.Motorization;
            existingVehicle.Mileage = vehicle.Mileage;
            existingVehicle.ListedAmount = vehicle.ListedAmount;
            existingVehicle.RentalTermMonths = vehicle.ListingType == ListingType.SALE ? null : vehicle.RentalTermMonths;
            existingVehicle.ListingType = vehicle.ListingType;
            existingVehicle.Status = vehicle.Status;
            existingVehicle.ImageUrl = vehicle.ImageUrl;
            existingVehicle.ImageKey = vehicle.ImageKey;
            existingVehicle.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
        }

        public async Task DeleteVehicleAsync(int id, ClaimsPrincipal user)
        {
            //check role
            if(user.IsInRole("Customer"))
            {
                throw new UnauthorizedAccessException("Action non autorisée pour les clients.");
            }

            var vehicle = await _context.Vehicles
                .Include(v => v.Applications)
                .FirstOrDefaultAsync(v => v.Id == id);

            if (vehicle == null)
            {
                throw new KeyNotFoundException($"Vehicle with id {id} not found.");
            }

            if (
                vehicle.Applications.Any(a => a.Status == ApplicationStatus.SUBMITTED || 
                a.Status == ApplicationStatus.ON_HOLD || 
                (a.Status == ApplicationStatus.APPROVED && a.Vehicle.ListingType == ListingType.RENTAL)
                )
            ){
                throw new InvalidOperationException("Action non autorisée pour les véhicules avec des candidatures ou des contrats en cours.");
            }

            _context.Vehicles.Remove(vehicle);
            await _context.SaveChangesAsync();
        }
    }
}