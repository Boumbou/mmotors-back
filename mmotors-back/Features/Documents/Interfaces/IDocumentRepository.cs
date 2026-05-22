/*
    * this file define the interface for documents repository
    * it will be used to define the methods that the repository will implement
*/

using mmotors_back.Models;
using mmotors_back.Features.Documents.Dtos;

namespace mmotors_back.Features.Documents.Interfaces
{
    public interface IDocumentRepository
    {
        Task<DocumentDto> GetDocumentByIdAsync(int id);
        Task UpdateDocumentAsync(DocumentDto document);
    }
}