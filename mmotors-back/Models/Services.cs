/*
    * this file defines the service entity
    * this entity is used to store services information
    * its properties are:
        int id PK
        string code UK
        string name
        string description
        string overheadType "PERCENTAGE, FIXED_AMOUNT"
        decimal overheadValue
        boolean isOptional
        boolean isActive
        datetime createdAt
        datetime updatedAt
*/

namespace mmotors_back.Models
{
    public class Service
    {
        public int Id { get; set; }
        public string Code { get; set; } = null!; // Unique code for the service
        public string Name { get; set; } = null!;
        public string Description { get; set; } = null!;
        public OverheadType OverheadType { get; set; }
        public decimal OverheadValue { get; set; }
        public bool IsOptional { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        // Navigation property
        public ICollection<ApplicationService> ApplicationServices { get; set; } = new List<ApplicationService>();
    }

    public enum OverheadType
    {
        PERCENTAGE,
        FIXED_AMOUNT
    }
}