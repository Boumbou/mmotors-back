/*
    * this file define the application entity
    * this entity is used to store the application information
    * its properties are:
        * int id PK
        * int userId FK
        * int vehicleId FK
        * int reviewedByUserId FK
        * string applicationType "PURCHASE, RENTAL"
        * string status "DRAFT, SUBMITTED, IN_REVIEW, APPROVED, REJECTED"
        * decimal baseAmount "snapshot of vehicle listedAmount"
        * decimal totalOverheadAmount "sum of selected ApplicationService amounts"
        * decimal totalAmount "baseAmount + totalOverheadAmount"
        * datetime submittedAt
        * datetime reviewedAt
        * string rejectionReason
        * datetime createdAt
        * datetime updatedAt
*/

namespace mmotors_back.Models
{
    public class Application
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int VehicleId { get; set; }
        public int? ReviewedByUserId { get; set; }
        public ListingType ApplicationType { get; set; } 
        public ApplicationStatus Status { get; set; } = ApplicationStatus.DRAFT;
        public decimal BaseAmount { get; set; }
        public decimal TotalOverheadAmount { get; set; } // Sum of selected ApplicationService amounts
        public decimal TotalAmount { get; set; } // BaseAmount + TotalOverheadAmount
        public DateTime? SubmittedAt { get; set; }
        public DateTime? ReviewedAt { get; set; }
        public string? RejectionReason { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        // Navigation properties
        public User User { get; set; } = null!;
        public Vehicle Vehicle { get; set; } = null!;
        public User? ReviewedByUser { get; set; }
        public ICollection<ApplicationService> ApplicationServices { get; set; } = new List<ApplicationService>();
    }

    public enum ApplicationStatus
    {
        DRAFT,
        SUBMITTED,
        IN_REVIEW,
        APPROVED,
        REJECTED
    }
}