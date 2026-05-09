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