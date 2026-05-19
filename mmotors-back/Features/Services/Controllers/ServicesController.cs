/*
    * this file defines the services controller
    * this controller is used to manage the services
    * its endpoints are:
        * GET /api/services - get all services
        * GET /api/services/{id} - get a service by id
        * POST /api/services - create a new service
        * PUT /api/services/{id} - update a service name description, overheadType, overheadValue, isOptional
        * DELETE /api/services/{id} - delete a service
        * POST /api/services/{id}/toggle - activate or deactivate a service
*/
using Microsoft.AspNetCore.Mvc;
using mmotors_back.Models;
using mmotors_back.Features.Services.Interfaces;
using mmotors_back.Features.Services.Dtos;
using Microsoft.AspNetCore.Authorization;

namespace mmotors_back.Features.Services.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ServicesController : ControllerBase
    {
        private readonly IServicesRepository _servicesRepository;

        public ServicesController(IServicesRepository servicesRepository)
        {
            _servicesRepository = servicesRepository;
        }

        // GET: api/services
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<ServiceDto>>> GetServices([FromQuery] ListingType? listingType = null)
        {
            var services = await _servicesRepository.GetAllServicesAsync(listingType);
            return Ok(services);
        }

        // GET: api/services/{id}
        [HttpGet("{id}")]
        [Authorize("RequireAdminRole")]
        public async Task<ActionResult<ServiceDto>> GetService(int id)
        {
            try
            {
                 var service = await _servicesRepository.GetServiceByIdAsync(id);
                 return Ok(service);
            }catch (KeyNotFoundException)
            {
                return NotFound();
            }catch (Exception)
            {
                return StatusCode(500, "An error occurred while retrieving the service.");
            }
        }

        // POST: api/services
        [HttpPost]
        [Authorize("RequireAdminRole")]
        public async Task<ActionResult<ServiceDto>> CreateService(CreateServiceDto service)
        {
            var createdService = await _servicesRepository.CreateServiceAsync(service);
            return CreatedAtAction(nameof(GetService), new { id = createdService.Id }, createdService);
        }

        // PUT: api/services/{id}
        [HttpPut("{id}")]
        [Authorize("RequireAdminRole")]
        public async Task<IActionResult> UpdateService(int id, ServiceDto service)
        {
            if (id != service.Id)
            {
                return BadRequest();
            }
            
            var updated = await _servicesRepository.UpdateServiceAsync(service);
            if (updated == 0)
            {
                return NotFound();
            }

            return NoContent();
        }

        // DELETE: api/services/{id}
        [HttpDelete("{id}")]
        [Authorize("RequireAdminRole")]
        public async Task<IActionResult> DeleteService(int id)
        {
            var deleted = await _servicesRepository.DeleteServiceAsync(id);
            if (deleted == 0)            {
                return NotFound();
            }

            return NoContent();
        }

        // POST: api/services/{id}/toggle
        [HttpPost("{id}/toggle")]
        [Authorize("RequireAdminRole")]
        public async Task<IActionResult> ToggleService(int id)
         {
             var toggled = await _servicesRepository.ToggleServiceStatusAsync(id);
             if (toggled == 0)
             {
                 return NotFound();
             }

             return NoContent();
         }  
     }
}
