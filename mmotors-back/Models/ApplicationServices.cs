/*
    * thise file defines the application service entity
    * this entity is used to store the selected services for each application
    * its properties are:
        int id PK
        int applicationId FK
        int serviceId FK
        boolean isSelected
        string appliedOverheadType "PERCENTAGE, FIXED_AMOUNT"
        decimal appliedOverheadValue
        decimal calculatedOverheadAmount
        datetime selectedAt
*/

namespace mmotors_back.Models
{
    public class ApplicationService
    {
        public int Id { get; set; }
        public int ApplicationId { get; set; }
        public int ServiceId { get; set; }
        public OverheadType AppliedOverheadType { get; set; }
        public decimal AppliedOverheadValue { get; set; }
        public decimal CalculatedOverheadAmount { get; set; }
        public DateTime? SelectedAt { get; set; }= DateTime.UtcNow;

        // Navigation properties
        public Application Application { get; set; } = null!;
        public Service Service { get; set; } = null!;
    }
}