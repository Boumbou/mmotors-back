/*
    * this file imlement de document repository
    * it will be used to interact with the database and perform CRUD operations on documents
*/

using mmotors_back.Data;
using mmotors_back.Features.Documents.Interfaces;
using mmotors_back.Features.Documents.Dtos;
using Microsoft.EntityFrameworkCore;

namespace mmotors_back.Features.Documents.Repositories
{
    public class DocumentRepository : IDocumentRepository
    {
        private readonly AppDbContext _context;

        public DocumentRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<DocumentDto> GetDocumentByIdAsync(int id)
        {
            var document = await _context.Documents.FirstOrDefaultAsync(d => d.Id == id);
            if (document == null)
            {
                throw new KeyNotFoundException($"Document with id {id} not found.");
            }
            return new DocumentDto
            {
                Id = document.Id,
                FileName = document.FileName,
                Url = document.Url,
                Extension = document.Extension,
                MimeType = document.MimeType,
                Key = document.Key,
                VehicleId = document.VehicleId
            };
        }

        public async Task UpdateDocumentAsync(DocumentDto documentDto)
        {
            var document = await _context.Documents.FindAsync(documentDto.Id);
            if (document == null)
            {
                throw new KeyNotFoundException($"Document with id {documentDto.Id} not found.");
            }
            document.FileName = documentDto.FileName;
            document.Url = documentDto.Url;
            document.Extension = documentDto.Extension;
            document.MimeType = documentDto.MimeType;
            document.VehicleId = documentDto.VehicleId;
            document.Key = documentDto.Key;

            _context.Documents.Update(document);
            await _context.SaveChangesAsync();
        }
    }
}