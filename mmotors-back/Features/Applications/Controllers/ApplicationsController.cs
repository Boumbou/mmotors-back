/*
    * this file defines the applications controller
    * this controller is used to handle the application related requests
    * its actions are:
        * POST /api/applications: create a new application OK
        * GET /api/applications/{id}: get application details by id OK
        * GET /api/applications: get all applications for the authenticated user OK
        * PUT /api/applications/{id}: update an application (only allowed in DRAFT and ON_HOLD status)
        * POST /api/applications/{id}/submit: submit an application for review (turn to SUBMITTED status)
        * POST /api/applications/{id}/hold: put an application on hold to request changes (turn to ON_HOLD status)
        * POST /api/applications/{id}/review: review an application (turn to APPROVED or REJECTED status)
        * it uses the IApplicationsRepository to interact with the db
*/

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using mmotors_back.Features.Applications.Interfaces;
using mmotors_back.Features.Applications.Services;
using mmotors_back.Features.Applications.Dtos;
using mmotors_back.Models;
using Microsoft.AspNetCore.Http.HttpResults;

namespace mmotors_back.Features.Applications.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ApplicationsController : ControllerBase
    {
        private readonly IApplicationsRepository _applicationsRepository;
        private readonly CheckAuthorization _checkAuthorization;

        public ApplicationsController(IApplicationsRepository applicationsRepository, CheckAuthorization checkAuthorization)
        {
            _applicationsRepository = applicationsRepository;
            _checkAuthorization = checkAuthorization;
        }

        [HttpPost]
        [Authorize(Policy = "RequireCustomerRole")]
        public async Task<IActionResult> CreateApplication([FromBody] CreateApplicationDto application)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                ApplicationDto createdApplication = await _applicationsRepository.CreateApplicationAsync(application);
                return CreatedAtAction(nameof(CreateApplication), new { id = createdApplication.Id }, createdApplication);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }
        
        [HttpGet("{id}")]
        [Authorize(Policy = "RequireAuthenticatedUser")]
        public async Task<IActionResult> GetApplicationById(int id)
        {
            if (id <= 0)
            {
                return BadRequest("ID de candidature invalide.");
            }

            //check if customer has access
            if(User.IsInRole("Customer"))
            {
                //only allow customers to access their own applications
                IActionResult authorizationResult = await _checkAuthorization.IsUserAuthorized(User, id);
                if (authorizationResult is not OkResult)
                {                    
                    return authorizationResult;
                }
            }

            try
            {
                ApplicationDto application = await _applicationsRepository.GetApplicationByIdAsync(id);
                return Ok(application);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpGet]
        [Authorize(Policy = "RequireAuthenticatedUser")]
        public async Task<IActionResult> GetAllApplications([FromQuery] PaginationParams? paginationParams = null)
        {
            PagedResults<ApplicationDto> applications = await _applicationsRepository.GetAllApplicationsAsync(paginationParams,User);
            return Ok(applications);
        }

        [HttpDelete("{id}")]
        [Authorize(Policy = "RequireAuthenticatedUser")]
        public async Task<IActionResult> DeleteApplication(int id)
        {
            if (id <= 0)
            {
                return BadRequest("ID de candidature invalide.");
            }

            //check if customer has access
            if(User.IsInRole("Customer"))
            {
                //only allow customers to access their own applications
                IActionResult authorizationResult = await _checkAuthorization.IsUserAuthorized(User, id);
                if (authorizationResult is not OkResult)
                {                    
                    return authorizationResult;
                }
            }

            try
            {
                await _applicationsRepository.DeleteApplicationAsync(id, User);
                return NoContent();
            }catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }catch(InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }catch(Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }


    }
}