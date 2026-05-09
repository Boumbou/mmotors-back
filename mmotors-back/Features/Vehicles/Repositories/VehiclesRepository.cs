/*
    * this file implement the repository for vehicles
    * it will be used to interact with the database and perform CRUD operations on vehicles
    * it will be injected into the controllers to handle the business logic related to vehicles
    * it will use the AppDbContext to interact with the database
    * it will implement the IVehiclesRepository interface to ensure that it has the necessary methods for handling vehicles
    * it will use the VehicleDto to transfer data between the database and the controllers
*/

namespace mmotors_back.Features.Vehicles.Repositories
{
    public class VehiclesRepository : IVehiclesRepository
    {
        private readonly AppDbContext _context;

        public VehiclesRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<VehicleDto>> GetAllVehiclesAsync()
        {
            throw new NotImplementedException();
        }

        public async Task<VehicleDto> GetVehicleByIdAsync(int id)
        {
            throw new NotImplementedException();
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