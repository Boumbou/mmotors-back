/*
    * this file implement the DTOs for vehicles based on the Vehicle entity for the followin scenarios:
    * get all vehicles
    * get vehicle by id
    * add vehicle
    * update vehicle
    * delete vehicle
*/

using System.ComponentModel.DataAnnotations;
using mmotors_back.Models;

namespace mmotors_back.Features.Vehicles.Dtos;

public class VehicleDto
{
    public int Id { get; set; }
    public string Brand { get; set; }
    public string Model { get; set; }
    public int Year { get; set; }
    public Motorization Motorization { get; set; }
    public int Mileage { get; set; }
    public decimal ListedAmount { get; set; }
    public RentalTerm? RentalTermMonths { get; set; } // Nullable, only for RENTAL
    public ListingType ListingType { get; set; } // SALE or RENTAL
    public VehicleStatus Status { get; set; } // AVAILABLE, SOLD, RENTED
    public string? ImageUrl { get; set; }
    public string? ImageKey { get; set; }
}

public class CreateVehicleDto
{
    [Required]
    //add validation details
    public string Brand { get; set; }
    [Required]
    public string Model { get; set; }
    [Required]
    public int Year { get; set; }
    [Required]
    public Motorization Motorization { get; set; }
    [Required]
    public int Mileage { get; set; }
    [Required]
    public decimal ListedAmount { get; set; }
    public RentalTerm? RentalTermMonths { get; set; } // Nullable, only for RENTAL
    [Required]
    public ListingType ListingType { get; set; } // SALE or RENTAL
    [DataType(DataType.Url)]
    public string? ImageUrl { get; set; }
    public string? ImageKey { get; set; }
}

public class UpdateVehicleDto
{
   [Required]
    public int Id { get; set; }
    public string? Brand { get; set; }
    public string? Model { get; set; }
    public int? Year { get; set; }
    public Motorization? Motorization { get; set; }
    public int? Mileage { get; set; }
    public decimal? ListedAmount { get; set; }
    public RentalTerm? RentalTermMonths { get; set; } // Nullable, only for RENTAL
    public ListingType? ListingType { get; set; } // SALE or RENTAL
    public VehicleStatus? Status { get; set; } // AVAILABLE, SOLD, RENTED
    [DataType(DataType.Url)]
    public string? ImageUrl { get; set; }
    public string? ImageKey { get; set; }
}