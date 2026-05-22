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
            if (vehicles.Items != null)
            {
                foreach (var vehicle in vehicles.Items)
                {
                    if (!string.IsNullOrEmpty(vehicle.ImageKey))
                    {
                        vehicle.ImageUrl = _storageService.GetFileUrl(vehicle.ImageKey, "01_vehicules");
                    }
                }
            }
            return Ok(vehicles);
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetVehicleById(int id)
        {
            try
            {
                var vehicle = await _vehiclesRepository.GetVehicleByIdAsync(id);
                if (!string.IsNullOrEmpty(vehicle.ImageKey))
                {
                    vehicle.ImageUrl = _storageService.GetFileUrl(vehicle.ImageKey, "01_vehicules");
                }
                return Ok(vehicle);
            }catch (KeyNotFoundException)
            {
                return NotFound();
            }

        }

        [HttpPost]
        [Authorize(policy: "RequireStaffOrAdminRole")]
        public async Task<IActionResult> AddVehicle([FromForm] CreateVehicleDto vehicle, [FromForm] IFormFile? image)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (image != null)
            {
                var uploadResult = await _storageService.UploadFileAsync(image, "01_vehicules");
                vehicle.ImageUrl = uploadResult.Url;
                vehicle.ImageKey = uploadResult.Key;
            }


            var createdVehicle = await _vehiclesRepository.AddVehicleAsync(vehicle, User);
            return CreatedAtAction(nameof(GetVehicleById), new { id = createdVehicle.Id }, createdVehicle);
        }

        [HttpPut("{id}")]
        [Authorize(policy: "RequireStaffOrAdminRole")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UpdateVehicle(int id, [FromForm] VehicleDto vehicle, [FromForm] IFormFile? image)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            if (id != vehicle.Id)
            {
                return BadRequest("Vehicle ID mismatch");
            }

            if(vehicle.ImageKey != null && image != null)
            {
                await _storageService.DeleteFileAsync(vehicle.ImageKey, "01_vehicules");
            }

            if (image != null)
            {
                var uploadResult = await _storageService.UploadFileAsync(image, "01_vehicules");
                vehicle.ImageUrl = uploadResult.Url;
                vehicle.ImageKey = uploadResult.Key;
            }
            await _vehiclesRepository.UpdateVehicleAsync(vehicle, User);
            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(policy: "RequireStaffOrAdminRole")]
        public async Task<IActionResult> DeleteVehicle(int id)
        {
            var vehicle = await _vehiclesRepository.GetVehicleByIdAsync(id);

            if (vehicle.ImageKey != null)
            {
                await _storageService.DeleteFileAsync(vehicle.ImageKey, "01_vehicules");
            }

            await _vehiclesRepository.DeleteVehicleAsync(id, User);
            return NoContent();
        }
    }
}