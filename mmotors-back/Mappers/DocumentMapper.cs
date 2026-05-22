/*
    * this file maps the document entity to the document dto
*/

using mmotors_back.Models;
using mmotors_back.Features.Documents.Dtos;

namespace mmotors_back.Mappers
{
    public static class DocumentMapper
    {
        public static DocumentDto ToDto(Document document) 
        {
            return new DocumentDto
            {
                Id = document.Id,
                ApplicationId = document.ApplicationId,
                VehicleId = document.VehicleId,
                FileName = document.FileName,
                MimeType = document.MimeType,
                Extension = document.Extension,
                Url = document.Url,
                Key = document.Key,
                UploadedAt = document.UploadedAt
            };
        }

        public static Document ToEntity(CreateDocumentDto documentDto) 
        {
            return new Document
            {
                ApplicationId = documentDto.ApplicationId,
                VehicleId = documentDto.VehicleId,
                FileName = documentDto.FileName,
                Type = documentDto.Type
            };
        }
    }
}   