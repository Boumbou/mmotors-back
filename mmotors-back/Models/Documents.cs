/*
    * this file defines the Document entity
    * this entity is used to store medias in the application
    * a document can be required for every application
    * a document can be required according to ListingType SALES or RENTAL
    * a document can also be of type vehicle photo to be displayed in the listing
*/

using Microsoft.AspNetCore.Mvc.Formatters;

namespace mmotors_back.Models
{
    public class Document
    {
        public int Id { get; set; }
        public required string FileName { get; set; }
        public string? Url { get; set; }
        public string? Key { get; set; } 
        public string? MimeType { get; set; } = null; // this is the type of the media (application/pdf, image/jpeg, etc.)
        public string? Extension { get; set; } = null; // this is the extension of the file (jpg, png, pdf, etc.)
        public DocumentType Type { get; set; }
        public DateTime UploadedAt { get; set; }= DateTime.UtcNow;
        public string? UploadedByUserId { get; set; } // the user who uploaded the document
        public int? ApplicationId { get; set; } //only if the document is required for an application
        public int? VehicleId { get; set; } //only if the document is a photo of the vehicle

        public Application? Application { get; set; } // navigation property to the application
        public Vehicle? Vehicle { get; set; } // navigation property to the vehicle
    }
}
