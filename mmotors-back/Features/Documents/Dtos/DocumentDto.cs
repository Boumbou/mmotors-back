/*
    * This file defines Dtos for documents
    * These Dtos are used to transfer data between the client and the server
    * The different Dtos are:
        * DocumentDto: to send document details to the client without the navigation properties
        * CreateDocumentDto: to receive document creation data from the client
        * UpdateDocumentDto: to receive document update data from the client
*/

using mmotors_back.Models;

namespace mmotors_back.Features.Documents.Dtos
{
    public class DocumentDto
    {
        public int Id { get; set; }
        public DocumentType Type { get; set; }
        public required string FileName { get; set; }
        public string? MimeType { get; set; }
        public string? Extension { get; set; }
        public string? Url { get; set; }
        public string? Key { get; set; }
        public int? ApplicationId { get; set; }
        public DateTime? UploadedAt { get; set; }
        public int? VehicleId { get; set; }

    }

    public class CreateDocumentDto
    {
        public DocumentType Type { get; set; }
        public required string FileName { get; set; }
        public int? ApplicationId { get; set; }
        public int? VehicleId { get; set; }
    }

    public class UpdateDocumentDto
    {
        public int Id { get; set; }
        public required string FileName { get; set; }
        public string? Url { get; set; }
        public string? Key { get; set; }
        public string? MimeType { get; set; }
        public string? Extension { get; set; }
        
    }
}