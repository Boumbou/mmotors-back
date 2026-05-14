/*
    * this file defines Dtos for applications
    * these Dtos are used to transfer data between the client and the server
    * the different Dtos are:
        * ApplicationDto: to send application details to the client
        * CreateApplicationDto: to receive application creation data from the client
        * UpdateApplicationDto: to receive application update data from the client
        * ReviewApplicationDto: to receive application review data from the client
*/

using mmotors_back.Models;

namespace mmotors_back.Features.Applications.Dtos
{
    public class ApplicationDto
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string? ReviewedByUserId { get; set; }
        public ListingType ApplicationType { get; set; }
        public int VehicleId { get; set; }
        public decimal? TotalAmount { get; set; }
        public ApplicationStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        //TODO: add the selected services in the application details response
        // List of selected services for this application
        // public List<ApplicationServiceDto> SelectedServices { get; set; } = new List<ApplicationServiceDto>();
    }

    public class CreateApplicationDto
    {
        public int VehicleId { get; set; }

        public string UserId { get; set; }
        public ListingType ApplicationType { get; set; }

        public decimal BaseAmount { get; set; }
        public decimal TotalOverheadAmount { get; set; }
    }

    public class UpdateApplicationDto
    {
        public int Id { get; set; }

        //TODO: define which fields can be updated in the application update request
        // only documents can be updated in the DRAFT status,
        //
    }

    public class ReviewApplicationDto
    {
        public int ApplicationId { get; set; }
        public string ReviewerUserId { get; set; }
        public bool IsApproved { get; set; }
        public string? RejectionReason { get; set; }
    }
}