/*
    * this file defines the class DocumentTemplate
    * this class is used to define the templates for the documents required for the applications
    * a document template can be required for every application
    * a document template can be required according to ListingType SALES or RENTAL
    * a document template can also be of type vehicle photo to be displayed in the listing
*/

namespace mmotors_back.Models
{
    public class DocumentTemplate
    {
        public int Id { get; set; }
        public string Name { get; set; } // the name of the document template (e.g. "Proof of Identity", "Vehicle Registration Certificate", etc.)
        public DocumentType Type { get; set; } // the type of the document template (e.g. VehiclePhoto, ApplicationDocument, SalesDocument, RentalDocument)
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; } = true; // indicates whether the document template is active and should be used for new applications
    
    }
    public enum DocumentType
    {
        VEHICLE_PHOTO, // for the listing
        COMMON_APPLICATION, // required for any application
        SALES_APPLICATION, // required for sales applications
        RENTAL_APPLICATION // required for rental applications
    }
}
