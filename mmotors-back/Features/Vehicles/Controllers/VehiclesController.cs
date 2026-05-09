/* 
    *this file implement the controller for vehicles
    * it is responsible for handling the HTTP requests related to vehicles and returning the appropriate responses
    * it uses the IVehiclesRepository to interact with the database and perform CRUD operations on vehicles
    * it uses the VehicleDto to transfer data between the database and the controllers
    * it is decorated with the [ApiController] attribute to enable automatic model validation and other API-specific features
    * it is decorated with the [Route] attribute to define the route for the controller
    * it is decorated with the [Authorize] attribute to require authentication for all actions
*/

namespace mmotors_back.Features.Vehicles.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class VehiclesController : ControllerBase
    {
        private readonly IVehiclesRepository _vehiclesRepository;

        public VehiclesController(IVehiclesRepository vehiclesRepository)
        {
            _vehiclesRepository = vehiclesRepository;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllVehicles()
        {
            var vehicles = await _vehiclesRepository.GetAllVehiclesAsync();
            return Ok(vehicles);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetVehicleById(int id)
        {
            var vehicle = await _vehiclesRepository.GetVehicleByIdAsync(id);
            if (vehicle == null)            {
                return NotFound();
            }
            return Ok(vehicle);
        }

        [HttpPost]
        [Authorize(Roles = "Staff")]
        public async Task<IActionResult> AddVehicle([FromBody] VehicleDto vehicle)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            await _vehiclesRepository.AddVehicleAsync(vehicle);
            return CreatedAtAction(nameof(GetVehicleById), new { id = vehicle.Id }, vehicle);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Staff")]
        public async Task<IActionResult> UpdateVehicle(int id, [FromBody] VehicleDto vehicle)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            if (id != vehicle.Id)
            {
                return BadRequest("Vehicle ID mismatch");
            }
            await _vehiclesRepository.UpdateVehicleAsync(vehicle);
            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Staff")]
        public async Task<IActionResult> DeleteVehicle(int id)
        {
            await _vehiclesRepository.DeleteVehicleAsync(id);
            return NoContent();
        }
    }
}