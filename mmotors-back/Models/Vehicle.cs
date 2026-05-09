/*
    * This file defines the vehicle class and implements the following properties:
    * int id PK
    * string brand
    * string model
    * string motorization
    * int mileage
    * decimal listedAmount "sale price or monthly rental amount"
    * int rentalTermMonths "24, 48 - only for RENTAL"
    * string listingType "SALE, RENTAL"
    * string status "AVAILABLE, RESERVED, SOLD, RENTED"
    * datetime createdAt
    * datetime updatedAt
*/

namespace mmotors_back.Models
{
    public class Vehicle
    {
        public int Id { get; set; }
        public string Brand { get; set; }
        public string Model { get; set; }
        public Motorization Motorization { get; set; }
        public int Mileage { get; set; }
        public decimal ListedAmount { get; set; }
        public RentalTerm? RentalTermMonths { get; set; } // Nullable, only for RENTAL
        public ListingType ListingType { get; set; } // SALE or RENTAL
        public VehicleStatus Status { get; set; } // AVAILABLE, SOLD, RENTED
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
    public enum ListingType
    {
        SALE,
        RENTAL
    }

    public enum VehicleStatus
    {
        AVAILABLE,
        SOLD,
        RENTED
    }

    public enum RentalTerm
    {
        Months24 = 24,
        Months48 = 48
    }

    public enum Motorization
    {
        Essence,
        Diesel,
        Électrique,
        Hybride
    }
}


