/*
    * this file implement the controller for application documents
    * it is responsible for handling the HTTP requests related to application documents
    * it will be used to upload, download and delete documents related to applications
    * it uses the IStorageService to handle the storage of documents
    * it receives the documents id and the document IformFile from the request and uses the IStorageService to handle the storage of the documents
*/

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using mmotors_back.Features.Shared.Interfaces;
using mmotors_back.Features.Documents.Interfaces;
using mmotors_back.Features.Applications.Dtos;
using mmotors_back.Mappers;

namespace mmotors_back.Features.Documents.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class DocumentsController : ControllerBase
    {
        private readonly IStorageService _storageService;
        private readonly IDocumentRepository _documentRepository;

        public DocumentsController(IStorageService storageService, IDocumentRepository documentRepository)
        {
            _storageService = storageService;
            _documentRepository = documentRepository;
        }

        // POST: api/documents/upload
        [HttpPost("upload")]
        [Authorize(policy:"RequireAuthenticatedUser")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UploadDocument([FromForm] int id,[FromForm] IFormFile? document)
        {
            Console.WriteLine("UPLOAD ACTION HIT");

            if (document == null || document.Length == 0)
            {
                return BadRequest("No document uploaded.");
            }

            Console.WriteLine("UPLOAD ACTION CONTINUED");
            //check if file exists for this document id
            var existingDocument = await _documentRepository.GetDocumentByIdAsync(id);

            if (existingDocument.Key != null)
            {
                //delete the existing file
                await _storageService.DeleteFileAsync(existingDocument.Key, "01_applications");
            }

            Console.WriteLine("FILE DELETED IF EXISTS");
            var result = await _storageService.UploadFileAsync(document, "01_applications");
            
            //update the document with the new file information
            existingDocument.Url = result.Url;
            existingDocument.Extension = Path.GetExtension(document.FileName);
            existingDocument.MimeType = document.ContentType;
            existingDocument.Key = result.Key;
            
            await _documentRepository.UpdateDocumentAsync(existingDocument);
            return Ok(existingDocument);
        }

        // GET: api/document/download/{key}
        [HttpGet("download")]
        [Authorize(policy:"RequireAuthenticatedUser")]
        public async Task<IActionResult> DownloadDocument([FromQuery] string key)
        {
            var stream = await _storageService.GetFileAsync(key, "01_applications");
            if (stream == null)
            {
                return NotFound();
            }
            return File(stream, "application/octet-stream", key);
        }

        // DELETE: api/document/{id}
        [HttpDelete("{id}")]
        [Authorize(policy:"RequireAuthenticatedUser")]
        public async Task<IActionResult> DeleteDocument(int id)
        {
            //find the document by id
            var document = await _documentRepository.GetDocumentByIdAsync(id);
            if (document == null)            {
                return NotFound();
            }

            if (document.Key != null)
            {
                await _storageService.DeleteFileAsync(document.Key, "01_applications");
                document.Key = null;
                document.Url = null;
                document.MimeType = null;
                document.Extension = null;
                await _documentRepository.UpdateDocumentAsync(document);
            }

            return NoContent();
        }
    }
}