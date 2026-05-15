/*
    * this file defines the service dto
    * this dto simplify the creation and update of services by the client
    * its dtos cover the following usecases:
        * create a new service (no navigation properties, no id, no timestamps)
        * update and send to client a service (name, description, overheadType, overheadValue, isOptional)
*/


using mmotors_back.Models;

namespace mmotors_back.Features.Services.Dtos
{
    public class CreateServiceDto
    {
        public string Name { get; set; } = null!;
        public string Description { get; set; } = null!;
        public ListingType ListingType { get; set; } // "SALE, RENTAL"
        public OverheadType OverheadType { get; set; }
        public decimal OverheadValue { get; set; }
        public bool IsOptional { get; set; } = true;
    }

    public class ServiceDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string Description { get; set; } = null!;
        public ListingType ListingType { get; set; } // "SALE, RENTAL"
        public OverheadType OverheadType { get; set; }
        public decimal OverheadValue { get; set; }
        public bool IsOptional { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}