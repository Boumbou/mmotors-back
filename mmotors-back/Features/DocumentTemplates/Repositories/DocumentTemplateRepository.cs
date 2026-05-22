/*
    * this file implements the repository for document templates
    * it will be used to interact with the database and perform CRUD operations on document templates
    * it will be injected into the controllers to handle the business logic related to document templates
    * it will use the AppDbContext to interact with the database
    * it will implement the IDocumentTemplateRepository interface to ensure that it has the necessary methods for handling document templates
*/

using mmotors_back.Data;
using mmotors_back.Features.DocumentTemplates.Interfaces;
using mmotors_back.Models;
using Microsoft.EntityFrameworkCore;

namespace mmotors_back.Features.DocumentTemplates.Repositories
{
    public class DocumentTemplateRepository : IDocumentTemplateRepository
    {
        private readonly AppDbContext _context;

        public DocumentTemplateRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<DocumentTemplate>> GetAllDocumentTemplatesAsync()
        {
            return await _context.DocumentTemplates.ToListAsync();
        }

        public async Task<DocumentTemplate> GetDocumentTemplateByIdAsync(int id)
        {
            var template = await _context.DocumentTemplates.FindAsync(id);
            if (template == null)
            {
                throw new KeyNotFoundException($"Modèle non trouvé avec l'ID {id}.");
            }   
            return template;
        }

        public async Task<DocumentTemplate> CreateDocumentTemplateAsync(DocumentTemplate template)
        {
            _context.DocumentTemplates.Add(template);
            await _context.SaveChangesAsync();
            return template;
        }

        public async Task<bool> UpdateDocumentTemplateAsync(DocumentTemplate template)
        {
            var existingTemplate = await _context.DocumentTemplates.FindAsync(template.Id);
            if (existingTemplate == null)
            {
                throw new KeyNotFoundException($"Modèle non trouvé avec l'ID {template.Id}.");
            }

            existingTemplate.Name = template.Name;
            existingTemplate.Type = template.Type;
            existingTemplate.IsActive = template.IsActive;
            existingTemplate.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteDocumentTemplateAsync(int id)
        {
            var template = await _context.DocumentTemplates.FindAsync(id);
            if (template == null)
            {
                throw new KeyNotFoundException($"Modèle non trouvé avec l'ID {id}.");
            }

            _context.DocumentTemplates.Remove(template);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}