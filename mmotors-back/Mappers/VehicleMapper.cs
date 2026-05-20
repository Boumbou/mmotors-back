/*
    * this file implements mappers for vehicle entity and its DTOs.
    * it maps following VehicleDtos:
    * - VehicleDto: for sending data to the client
    * - CreateVehicleDto: for receiving data from the client when creating a vehicle
    * - UpdateVehicleDto: for receiving data from the client when updating a vehicle
*/

using mmotors_back.Models;
using mmotors_back.Features.Vehicles.Dtos;

namespace mmotors_back.Mappers
{
    public class VehicleMapper
    {
        public static VehicleDto ToDTO(Vehicle vehicle, bool includeApplications = false)
        {
            return new VehicleDto
            {
                Id = vehicle.Id,
                Brand = vehicle.Brand,
                Model = vehicle.Model,
                Year = vehicle.Year,
                Motorization = vehicle.Motorization,
                Mileage = vehicle.Mileage,
                ListedAmount = vehicle.ListedAmount,
                RentalTermMonths = vehicle.RentalTermMonths,
                ListingType = vehicle.ListingType,
                Status = vehicle.Status,
                ImageUrl = vehicle.ImageUrl,
                ImageKey = vehicle.ImageKey,
                Applications = includeApplications && vehicle.Applications != null ? vehicle.Applications.Select(a => ApplicationMapper.ToDto(a)).ToList() : null // Map applications if they exist
            };
        }

        public static Vehicle ToEntity(CreateVehicleDto createVehicleDto)
        {
            return new Vehicle
            {
                Brand = createVehicleDto.Brand,
                Model = createVehicleDto.Model,
                Year = createVehicleDto.Year,
                Motorization = createVehicleDto.Motorization,
                Mileage = createVehicleDto.Mileage,
                ListedAmount = createVehicleDto.ListedAmount,
                RentalTermMonths = createVehicleDto.RentalTermMonths,
                ListingType = createVehicleDto.ListingType,
                ImageUrl = createVehicleDto.ImageUrl ?? null,
                ImageKey = createVehicleDto.ImageKey ?? null
            };
        }
    }
}