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

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using CarRental.Api.Data;
using CarRental.Api.Models;

namespace CarRental.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize] // Require Bearer token authentication for all endpoints
public class RentalsController : ControllerBase
{
    private readonly CarRentalDbContext _context;
    private readonly ILogger<RentalsController> _logger;

    public RentalsController(CarRentalDbContext context, ILogger<RentalsController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Get all rentals
    /// </summary>
    /// <param name="customerId">Filter by customer ID</param>
    /// <param name="companyId">Filter by company ID</param>
    /// <param name="vehicleId">Filter by vehicle ID</param>
    /// <param name="status">Filter by rental status</param>
    /// <param name="page">Page number</param>
    /// <param name="pageSize">Page size</param>
    /// <returns>List of rentals</returns>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<Rental>), 200)]
    [ProducesResponseType(401)] // Unauthorized
    public async Task<IActionResult> GetRentals(
        [FromQuery] Guid? customerId = null,
        [FromQuery] Guid? companyId = null,
        [FromQuery] Guid? vehicleId = null,
        [FromQuery] string? status = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        try
        {
            var query = _context.Rentals
                .Include(r => r.Customer)
                .Include(r => r.Vehicle)
                .Include(r => r.Company)
                .Include(r => r.Reservation)
                .AsQueryable();

            if (customerId.HasValue)
                query = query.Where(r => r.CustomerId == customerId.Value);

            if (companyId.HasValue)
                query = query.Where(r => r.CompanyId == companyId.Value);

            if (vehicleId.HasValue)
                query = query.Where(r => r.VehicleId == vehicleId.Value);

            if (!string.IsNullOrEmpty(status))
                query = query.Where(r => r.Status == status);

            var totalCount = await query.CountAsync();

            var rentals = await query
                .OrderByDescending(r => r.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return Ok(new
            {
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize,
                Rentals = rentals
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting rentals");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Get a specific rental by ID
    /// </summary>
    /// <param name="id">Rental ID</param>
    /// <returns>Rental details</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(Rental), 200)]
    [ProducesResponseType(404)]
    [ProducesResponseType(401)] // Unauthorized
    public async Task<IActionResult> GetRental(Guid id)
    {
        try
        {
            var rental = await _context.Rentals
                .Include(r => r.Customer)
                .Include(r => r.Vehicle)
                .Include(r => r.Company)
                .Include(r => r.Reservation)
                .FirstOrDefaultAsync(r => r.RentalId == id);

            if (rental == null)
                return NotFound();

            return Ok(rental);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting rental {RentalId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Create a new rental (start rental from reservation)
    /// </summary>
    /// <param name="rental">Rental details</param>
    /// <returns>Created rental</returns>
    [HttpPost]
    [ProducesResponseType(typeof(Rental), 201)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)] // Unauthorized
    public async Task<IActionResult> CreateRental([FromBody] Rental rental)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Validate that reservation exists and is confirmed
            var reservation = await _context.Reservations.FindAsync(rental.ReservationId);
            if (reservation == null)
                return BadRequest("Reservation not found");

            if (reservation.Status != "confirmed")
                return BadRequest("Reservation must be confirmed to start rental");

            // Validate that customer exists
            var customer = await _context.Customers.FindAsync(rental.CustomerId);
            if (customer == null)
                return BadRequest("Customer not found");

            // Validate that vehicle exists and is available
            var vehicle = await _context.Vehicles.FindAsync(rental.VehicleId);
            if (vehicle == null)
                return BadRequest("Vehicle not found");

            if (vehicle.Status != "available")
                return BadRequest("Vehicle is not available");

            // Validate that company exists
            var company = await _context.RentalCompanies.FindAsync(rental.CompanyId);
            if (company == null)
                return BadRequest("Company not found");

            rental.RentalId = Guid.NewGuid();
            rental.CreatedAt = DateTime.UtcNow;
            rental.UpdatedAt = DateTime.UtcNow;

            // Update vehicle status to rented
            vehicle.Status = "rented";
            vehicle.UpdatedAt = DateTime.UtcNow;

            // Update reservation status to active
            reservation.Status = "active";
            reservation.UpdatedAt = DateTime.UtcNow;

            _context.Rentals.Add(rental);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Created rental {RentalId} for customer {CustomerId}", 
                rental.RentalId, rental.CustomerId);

            return CreatedAtAction(nameof(GetRental), new { id = rental.RentalId }, rental);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating rental");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Update a rental
    /// </summary>
    /// <param name="id">Rental ID</param>
    /// <param name="rental">Updated rental details</param>
    /// <returns>Updated rental</returns>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(Rental), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    [ProducesResponseType(401)] // Unauthorized
    public async Task<IActionResult> UpdateRental(Guid id, [FromBody] Rental rental)
    {
        try
        {
            if (id != rental.RentalId)
                return BadRequest("ID mismatch");

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var existingRental = await _context.Rentals.FindAsync(id);
            if (existingRental == null)
                return NotFound();

            // Update properties
            existingRental.ActualPickupDate = rental.ActualPickupDate;
            existingRental.ExpectedReturnDate = rental.ExpectedReturnDate;
            existingRental.ActualReturnDate = rental.ActualReturnDate;
            existingRental.PickupMileage = rental.PickupMileage;
            existingRental.ReturnMileage = rental.ReturnMileage;
            existingRental.FuelLevelPickup = rental.FuelLevelPickup;
            existingRental.FuelLevelReturn = rental.FuelLevelReturn;
            existingRental.DamageNotesPickup = rental.DamageNotesPickup;
            existingRental.DamageNotesReturn = rental.DamageNotesReturn;
            existingRental.AdditionalCharges = rental.AdditionalCharges;
            existingRental.Status = rental.Status;
            existingRental.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Updated rental {RentalId}", id);

            return Ok(existingRental);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating rental {RentalId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Complete a rental (return vehicle)
    /// </summary>
    /// <param name="id">Rental ID</param>
    /// <param name="returnData">Return data</param>
    /// <returns>Updated rental</returns>
    [HttpPost("{id}/complete")]
    [ProducesResponseType(typeof(Rental), 200)]
    [ProducesResponseType(404)]
    [ProducesResponseType(401)] // Unauthorized
    public async Task<IActionResult> CompleteRental(Guid id, [FromBody] CompleteRentalRequest returnData)
    {
        try
        {
            var rental = await _context.Rentals
                .Include(r => r.Vehicle)
                .Include(r => r.Reservation)
                .FirstOrDefaultAsync(r => r.RentalId == id);

            if (rental == null)
                return NotFound();

            if (rental.Status != "active")
                return BadRequest("Rental is not active");

            // Update rental with return data
            rental.ActualReturnDate = returnData.ActualReturnDate ?? DateTime.UtcNow;
            rental.ReturnMileage = returnData.ReturnMileage;
            rental.FuelLevelReturn = returnData.FuelLevelReturn;
            rental.DamageNotesReturn = returnData.DamageNotesReturn;
            rental.AdditionalCharges = returnData.AdditionalCharges ?? 0;
            rental.Status = "completed";
            rental.UpdatedAt = DateTime.UtcNow;

            // Update vehicle status to available
            rental.Vehicle.Status = "available";
            rental.Vehicle.Mileage = returnData.ReturnMileage ?? rental.Vehicle.Mileage;
            rental.Vehicle.UpdatedAt = DateTime.UtcNow;

            // Update reservation status to completed
            rental.Reservation.Status = "completed";
            rental.Reservation.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Completed rental {RentalId}", id);

            return Ok(rental);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error completing rental {RentalId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Get active rentals
    /// </summary>
    /// <param name="companyId">Filter by company ID</param>
    /// <returns>List of active rentals</returns>
    [HttpGet("active")]
    [ProducesResponseType(typeof(IEnumerable<Rental>), 200)]
    [ProducesResponseType(401)] // Unauthorized
    public async Task<IActionResult> GetActiveRentals([FromQuery] Guid? companyId = null)
    {
        try
        {
            var query = _context.Rentals
                .Include(r => r.Customer)
                .Include(r => r.Vehicle)
                .Include(r => r.Company)
                .Where(r => r.Status == "active")
                .AsQueryable();

            if (companyId.HasValue)
                query = query.Where(r => r.CompanyId == companyId.Value);

            var activeRentals = await query
                .OrderBy(r => r.ExpectedReturnDate)
                .ToListAsync();

            return Ok(activeRentals);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting active rentals");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Get overdue rentals
    /// </summary>
    /// <param name="companyId">Filter by company ID</param>
    /// <returns>List of overdue rentals</returns>
    [HttpGet("overdue")]
    [ProducesResponseType(typeof(IEnumerable<Rental>), 200)]
    [ProducesResponseType(401)] // Unauthorized
    public async Task<IActionResult> GetOverdueRentals([FromQuery] Guid? companyId = null)
    {
        try
        {
            var query = _context.Rentals
                .Include(r => r.Customer)
                .Include(r => r.Vehicle)
                .Include(r => r.Company)
                .Where(r => r.Status == "active" && r.ExpectedReturnDate < DateTime.UtcNow)
                .AsQueryable();

            if (companyId.HasValue)
                query = query.Where(r => r.CompanyId == companyId.Value);

            var overdueRentals = await query
                .OrderBy(r => r.ExpectedReturnDate)
                .ToListAsync();

            return Ok(overdueRentals);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting overdue rentals");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Get rental statistics
    /// </summary>
    /// <param name="companyId">Company ID for statistics</param>
    /// <param name="startDate">Start date for statistics</param>
    /// <param name="endDate">End date for statistics</param>
    /// <returns>Rental statistics</returns>
    [HttpGet("statistics")]
    [ProducesResponseType(typeof(object), 200)]
    [ProducesResponseType(401)] // Unauthorized
    public async Task<IActionResult> GetRentalStatistics(
        [FromQuery] Guid? companyId = null,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        try
        {
            var query = _context.Rentals.AsQueryable();

            if (companyId.HasValue)
                query = query.Where(r => r.CompanyId == companyId.Value);

            if (startDate.HasValue)
                query = query.Where(r => r.CreatedAt >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(r => r.CreatedAt <= endDate.Value);

            var totalRentals = await query.CountAsync();
            var activeRentals = await query.CountAsync(r => r.Status == "active");
            var completedRentals = await query.CountAsync(r => r.Status == "completed");
            var overdueRentals = await query.CountAsync(r => r.Status == "active" && r.ExpectedReturnDate < DateTime.UtcNow);

            return Ok(new
            {
                TotalRentals = totalRentals,
                ActiveRentals = activeRentals,
                CompletedRentals = completedRentals,
                OverdueRentals = overdueRentals,
                GeneratedAt = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting rental statistics");
            return StatusCode(500, "Internal server error");
        }
    }
}

// DTO for completing rentals
public class CompleteRentalRequest
{
    public DateTime? ActualReturnDate { get; set; }
    public int? ReturnMileage { get; set; }
    public string? FuelLevelReturn { get; set; }
    public string? DamageNotesReturn { get; set; }
    public decimal? AdditionalCharges { get; set; }
}
