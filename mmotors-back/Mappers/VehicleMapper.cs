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
        public static VehicleDto ToDTO(Vehicle vehicle)
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
                ImageKey = vehicle.ImageKey
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
                ImageUrl = createVehicleDto.ImageUrl,
                ImageKey = createVehicleDto.ImageKey
            };
        }
    }
}