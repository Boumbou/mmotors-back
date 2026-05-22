/*
    *This file defines the DocumentTemplateController class
    *This controller is responsible for handling HTTP requests related to document templates
    *It will use the IDocumentTemplateRepository to interact with the database and perform CRUD operations
    *its methods are :
    * - GetAllDocumentTemplatesAsync: to get all document templates
    * - GetDocumentTemplateByIdAsync: to get a document template by its id
    * - CreateDocumentTemplateAsync: to create a new document template
    * - UpdateDocumentTemplateAsync: to update an existing document template
    * - DeleteDocumentTemplateAsync: to delete a document template by its id
*/

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using mmotors_back.Features.DocumentTemplates.Dtos;
using mmotors_back.Features.DocumentTemplates.Interfaces;
using mmotors_back.Models;

namespace mmotors_back.Features.DocumentTemplates.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Policy = "RequireAdminRole")]
    public class DocumentTemplateController : ControllerBase
    {
        private readonly IDocumentTemplateRepository _repository;

        public DocumentTemplateController(IDocumentTemplateRepository repository)
        {
            _repository = repository;
        }

        // GET: api/DocumentTemplate
        [HttpGet]
        public async Task<ActionResult<IEnumerable<DocumentTemplate>>> GetAllDocumentTemplatesAsync()
        {
            var templates = await _repository.GetAllDocumentTemplatesAsync();
            return Ok(templates);
        }

        // GET: api/DocumentTemplate/5
        [HttpGet("{id}")]
        public async Task<ActionResult<DocumentTemplate>> GetDocumentTemplateByIdAsync(int id)
        {
            var template = await _repository.GetDocumentTemplateByIdAsync(id);
            if (template == null)
            {
                return NotFound();
            }
            return Ok(template);
        }

        // POST: api/DocumentTemplate
        [HttpPost]
        public async Task<ActionResult<DocumentTemplate>> CreateDocumentTemplateAsync([FromBody] DocumentTemplateDto template)
        {   
            try
            {
                var createdTemplate = await _repository.CreateDocumentTemplateAsync(template);
                return CreatedAtAction("GetDocumentTemplateById", new { id = createdTemplate.Id }, createdTemplate);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // PUT: api/DocumentTemplate/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateDocumentTemplateAsync(int id, DocumentTemplateDto template)
        {
            if (id != template.Id)
            {
                return BadRequest();
            }

            var updated = await _repository.UpdateDocumentTemplateAsync(template);
            if (!updated)
            {
                return NotFound();
            }
            return NoContent();
        }

        // DELETE: api/DocumentTemplate/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDocumentTemplateAsync(int id)
        {
            var deleted = await _repository.DeleteDocumentTemplateAsync(id);
            if (!deleted)
            {
                return NotFound();
            }
            return NoContent();
        }
    }
}