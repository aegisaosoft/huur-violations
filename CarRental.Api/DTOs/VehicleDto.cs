/*
 *
 * Copyright (c) 2025 Alexander Orlov.
 * 34 Middletown Ave Atlantic Highlands NJ 07716
 *
 * THIS SOFTWARE IS THE CONFIDENTIAL AND PROPRIETARY INFORMATION OF
 * Alexander Orlov. ("CONFIDENTIAL INFORMATION"). YOU SHALL NOT DISCLOSE
 * SUCH CONFIDENTIAL INFORMATION AND SHALL USE IT ONLY IN ACCORDANCE
 * WITH THE TERMS OF THE LICENSE AGREEMENT YOU ENTERED INTO WITH
 * Alexander Orlov.
 *
 * Author: Alexander Orlov
 *
 */

namespace CarRental.Api.DTOs;

public class VehicleDto
{
    public Guid VehicleId { get; set; }
    public Guid CompanyId { get; set; }
    public string CompanyName { get; set; } = string.Empty;
    public Guid? CategoryId { get; set; }
    public string? CategoryName { get; set; }
    public string Make { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public int Year { get; set; }
    public string? Color { get; set; }
    public string LicensePlate { get; set; } = string.Empty;
    public string? Vin { get; set; }
    public int Mileage { get; set; }
    public string? FuelType { get; set; }
    public string? Transmission { get; set; }
    public int? Seats { get; set; }
    public decimal DailyRate { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Location { get; set; }
    public string? ImageUrl { get; set; }
    public string[]? Features { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class CreateVehicleDto
{
    public Guid CompanyId { get; set; }
    public Guid? CategoryId { get; set; }
    public string Make { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public int Year { get; set; }
    public string? Color { get; set; }
    public string LicensePlate { get; set; } = string.Empty;
    public string? Vin { get; set; }
    public int Mileage { get; set; } = 0;
    public string? FuelType { get; set; }
    public string? Transmission { get; set; }
    public int? Seats { get; set; }
    public decimal DailyRate { get; set; }
    public string Status { get; set; } = "available";
    public string? Location { get; set; }
    public string? ImageUrl { get; set; }
    public string[]? Features { get; set; }
    public bool IsActive { get; set; } = true;
}

public class UpdateVehicleDto
{
    public Guid? CategoryId { get; set; }
    public string? Color { get; set; }
    public string? Vin { get; set; }
    public int? Mileage { get; set; }
    public string? FuelType { get; set; }
    public string? Transmission { get; set; }
    public int? Seats { get; set; }
    public decimal? DailyRate { get; set; }
    public string? Status { get; set; }
    public string? Location { get; set; }
    public string? ImageUrl { get; set; }
    public string[]? Features { get; set; }
    public bool? IsActive { get; set; }
}
