/* 
    *this file implement the controller for vehicles
    * it is responsible for handling the HTTP requests related to vehicles and returning the appropriate responses
    * it uses the IVehiclesRepository to interact with the database and perform CRUD operations on vehicles
    * it uses the VehicleDto to transfer data between the database and the controllers
    * it is decorated with the [ApiController] attribute to enable automatic model validation and other API-specific features
    * it is decorated with the [Route] attribute to define the route for the controller
    * it is decorated with the [Authorize] attribute to require authentication for all actions
*/

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using mmotors_back.Features.Vehicles.Interfaces;
using mmotors_back.Features.Vehicles.Dtos;
using mmotors_back.Models;
using mmotors_back.Features.Shared.Interfaces;

namespace mmotors_back.Features.Vehicles.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class VehiclesController : ControllerBase
    {
        private readonly IVehiclesRepository _vehiclesRepository;
        private readonly IStorageService _storageService;

        public VehiclesController(IVehiclesRepository vehiclesRepository, IStorageService storageService)
        {
            _vehiclesRepository = vehiclesRepository;
            _storageService = storageService;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAllVehicles([FromQuery] string? type = null, [FromQuery] PaginationParams? paginationParams = null)
        {
            
            PaginationParams queryPaginationParams = paginationParams ?? new PaginationParams();
            var vehicles = await _vehiclesRepository.GetAllVehiclesAsync(type, queryPaginationParams);
            return Ok(vehicles);
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetVehicleById(int id)
        {
            try
            {
                var vehicle = await _vehiclesRepository.GetVehicleByIdAsync(id);
                return Ok(vehicle);
            }catch (KeyNotFoundException)
            {
                return NotFound();
            }

        }

        [HttpPost]
        [Authorize(Roles = "Staff")]
        public async Task<IActionResult> AddVehicle([FromBody] CreateVehicleDto vehicle, [FromBody] IFormFile? image)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (image != null)
            {
                var uploadResult = await _storageService.UploadFileAsync(image);
                vehicle.ImageUrl = uploadResult.Url;
                vehicle.ImageKey = uploadResult.Key;
            }


            var createdVehicle = await _vehiclesRepository.AddVehicleAsync(vehicle, User);
            return CreatedAtAction(nameof(GetVehicleById), new { id = createdVehicle.Id }, createdVehicle);
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
            await _vehiclesRepository.UpdateVehicleAsync(vehicle, User);
            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Staff")]
        public async Task<IActionResult> DeleteVehicle(int id)
        {
            await _vehiclesRepository.DeleteVehicleAsync(id, User);
            return NoContent();
        }
    }
}