/*
    * this file define the interface for the document template repository
    * it will be used to interact with the database and perform CRUD operations on document templates
    * it will be implemented by the DocumentTemplateRepository class to handle the business logic
    * its methods are :
    * - GetAllDocumentTemplatesAsync: to get all document templates
    * - GetDocumentTemplateByIdAsync: to get a document template by its id
    * - CreateDocumentTemplateAsync: to create a new document template
    * - UpdateDocumentTemplateAsync: to update an existing document template
    * - DeleteDocumentTemplateAsync: to delete a document template by its id
*/
using mmotors_back.Features.DocumentTemplates.Dtos;
using mmotors_back.Models;

namespace mmotors_back.Features.DocumentTemplates.Interfaces
{
    public interface IDocumentTemplateRepository
    {
        Task<IEnumerable<DocumentTemplate>> GetAllDocumentTemplatesAsync();
        Task<DocumentTemplate> GetDocumentTemplateByIdAsync(int id);
        Task<DocumentTemplate> CreateDocumentTemplateAsync(DocumentTemplateDto template);
        Task<bool> UpdateDocumentTemplateAsync(DocumentTemplateDto template);
        Task<bool> DeleteDocumentTemplateAsync(int id);
    }
}