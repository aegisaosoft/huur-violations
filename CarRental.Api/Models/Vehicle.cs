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

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CarRental.Api.Models;

[Table("vehicles")]
public class Vehicle
{
    [Key]
    [Column("vehicle_id")]
    public Guid VehicleId { get; set; } = Guid.NewGuid();

    [Required]
    [Column("company_id")]
    public Guid CompanyId { get; set; }

    [Column("category_id")]
    public Guid? CategoryId { get; set; }

    [Required]
    [MaxLength(100)]
    public string Make { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string Model { get; set; } = string.Empty;

    [Required]
    public int Year { get; set; }

    [MaxLength(50)]
    public string? Color { get; set; }

    [Required]
    [MaxLength(50)]
    [Column("license_plate")]
    public string LicensePlate { get; set; } = string.Empty;

    [MaxLength(17)]
    public string? Vin { get; set; }

    public int Mileage { get; set; } = 0;

    [MaxLength(50)]
    [Column("fuel_type")]
    public string? FuelType { get; set; }

    [MaxLength(50)]
    public string? Transmission { get; set; }

    public int? Seats { get; set; }

    [Required]
    [Column("daily_rate", TypeName = "decimal(10,2)")]
    public decimal DailyRate { get; set; }

    [MaxLength(50)]
    public string Status { get; set; } = "available";

    [MaxLength(255)]
    public string? Location { get; set; }

    [Column("image_url")]
    public string? ImageUrl { get; set; }

    public string[]? Features { get; set; }

    [Column("is_active")]
    public bool IsActive { get; set; } = true;

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    [ForeignKey("CompanyId")]
    public virtual RentalCompany Company { get; set; } = null!;

    [ForeignKey("CategoryId")]
    public virtual VehicleCategory? Category { get; set; }

    public virtual ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
    public virtual ICollection<Rental> Rentals { get; set; } = new List<Rental>();
    public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();
}
