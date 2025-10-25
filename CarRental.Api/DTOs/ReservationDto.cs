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

public class ReservationDto
{
    public Guid ReservationId { get; set; }
    public Guid CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;
    public Guid VehicleId { get; set; }
    public string VehicleName { get; set; } = string.Empty;
    public string LicensePlate { get; set; } = string.Empty;
    public Guid CompanyId { get; set; }
    public string CompanyName { get; set; } = string.Empty;
    public string ReservationNumber { get; set; } = string.Empty;
    public DateTime PickupDate { get; set; }
    public DateTime ReturnDate { get; set; }
    public string? PickupLocation { get; set; }
    public string? ReturnLocation { get; set; }
    public decimal DailyRate { get; set; }
    public int TotalDays { get; set; }
    public decimal Subtotal { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal InsuranceAmount { get; set; }
    public decimal AdditionalFees { get; set; }
    public decimal TotalAmount { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class CreateReservationDto
{
    public Guid CustomerId { get; set; }
    public Guid VehicleId { get; set; }
    public Guid CompanyId { get; set; }
    public DateTime PickupDate { get; set; }
    public DateTime ReturnDate { get; set; }
    public string? PickupLocation { get; set; }
    public string? ReturnLocation { get; set; }
    public decimal DailyRate { get; set; }
    public decimal TaxAmount { get; set; } = 0;
    public decimal InsuranceAmount { get; set; } = 0;
    public decimal AdditionalFees { get; set; } = 0;
    public string? Notes { get; set; }
}

public class UpdateReservationDto
{
    public DateTime? PickupDate { get; set; }
    public DateTime? ReturnDate { get; set; }
    public string? PickupLocation { get; set; }
    public string? ReturnLocation { get; set; }
    public decimal? DailyRate { get; set; }
    public decimal? TaxAmount { get; set; }
    public decimal? InsuranceAmount { get; set; }
    public decimal? AdditionalFees { get; set; }
    public string? Status { get; set; }
    public string? Notes { get; set; }
}
