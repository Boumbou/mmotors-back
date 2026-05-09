/*
    * this file implement the interface for the vehicles repository
    * it will be used to define the methods that the repository will implement
    * it will be injected into the controllers to handle the business logic related to vehicles
    * it will use the VehicleDto to transfer data between the database and the controllers
*/
using mmotors_back.Features.Vehicles.Dtos;

namespace mmotors_back.Features.Vehicles.Interfaces
{
    public interface IVehiclesRepository
    {
        Task<IEnumerable<VehicleDto>> GetAllVehiclesAsync();
        Task<VehicleDto> GetVehicleByIdAsync(int id);
        Task AddVehicleAsync(VehicleDto vehicle);
        Task UpdateVehicleAsync(VehicleDto vehicle);
        Task DeleteVehicleAsync(int id);
    }
}