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
        public string Name { get; set; } = null!;
        public string Description { get; set; } = null!;
        public ListingType ListingType { get; set; } // "SALE, RENTAL"
        public OverheadType OverheadType { get; set; }
        public decimal OverheadValue { get; set; }
        public bool IsOptional { get; set; } = true;
        public bool IsActive { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation property
        public ICollection<ApplicationService> ApplicationServices { get; set; } = new List<ApplicationService>();
    }

    public enum OverheadType
    {
        PERCENTAGE,
        FIXED_AMOUNT
    }
}