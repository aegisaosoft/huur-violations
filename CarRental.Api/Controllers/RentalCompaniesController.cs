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
public class RentalCompaniesController : ControllerBase
{
    private readonly CarRentalDbContext _context;
    private readonly ILogger<RentalCompaniesController> _logger;

    public RentalCompaniesController(CarRentalDbContext context, ILogger<RentalCompaniesController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Get all rental companies
    /// </summary>
    /// <param name="isActive">Filter by active status</param>
    /// <param name="page">Page number</param>
    /// <param name="pageSize">Page size</param>
    /// <returns>List of rental companies</returns>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<RentalCompany>), 200)]
    [ProducesResponseType(401)] // Unauthorized
    public async Task<IActionResult> GetRentalCompanies(
        [FromQuery] bool? isActive = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        try
        {
            var query = _context.RentalCompanies.AsQueryable();

            if (isActive.HasValue)
                query = query.Where(rc => rc.IsActive == isActive.Value);

            var totalCount = await query.CountAsync();

            var companies = await query
                .OrderBy(rc => rc.CompanyName)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return Ok(new
            {
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize,
                Companies = companies
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting rental companies");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Get a specific rental company by ID
    /// </summary>
    /// <param name="id">Company ID</param>
    /// <returns>Rental company details</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(RentalCompany), 200)]
    [ProducesResponseType(404)]
    [ProducesResponseType(401)] // Unauthorized
    public async Task<IActionResult> GetRentalCompany(Guid id)
    {
        try
        {
            var company = await _context.RentalCompanies
                .FirstOrDefaultAsync(rc => rc.CompanyId == id);

            if (company == null)
                return NotFound();

            return Ok(company);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting rental company {CompanyId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Create a new rental company
    /// </summary>
    /// <param name="company">Company details</param>
    /// <returns>Created company</returns>
    [HttpPost]
    [ProducesResponseType(typeof(RentalCompany), 201)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)] // Unauthorized
    public async Task<IActionResult> CreateRentalCompany([FromBody] RentalCompany company)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Check if email already exists
            if (await _context.RentalCompanies.AnyAsync(rc => rc.Email == company.Email))
            {
                return BadRequest("A company with this email already exists");
            }

            company.CompanyId = Guid.NewGuid();
            company.CreatedAt = DateTime.UtcNow;
            company.UpdatedAt = DateTime.UtcNow;

            _context.RentalCompanies.Add(company);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Created rental company {CompanyId} with name {CompanyName}", 
                company.CompanyId, company.CompanyName);

            return CreatedAtAction(nameof(GetRentalCompany), new { id = company.CompanyId }, company);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating rental company");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Update a rental company
    /// </summary>
    /// <param name="id">Company ID</param>
    /// <param name="company">Updated company details</param>
    /// <returns>Updated company</returns>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(RentalCompany), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    [ProducesResponseType(401)] // Unauthorized
    public async Task<IActionResult> UpdateRentalCompany(Guid id, [FromBody] RentalCompany company)
    {
        try
        {
            if (id != company.CompanyId)
                return BadRequest("ID mismatch");

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var existingCompany = await _context.RentalCompanies.FindAsync(id);
            if (existingCompany == null)
                return NotFound();

            // Check if email already exists for another company
            if (await _context.RentalCompanies.AnyAsync(rc => rc.Email == company.Email && rc.CompanyId != id))
            {
                return BadRequest("A company with this email already exists");
            }

            // Update properties
            existingCompany.CompanyName = company.CompanyName;
            existingCompany.Email = company.Email;
            existingCompany.Phone = company.Phone;
            existingCompany.Address = company.Address;
            existingCompany.City = company.City;
            existingCompany.State = company.State;
            existingCompany.Country = company.Country;
            existingCompany.PostalCode = company.PostalCode;
            existingCompany.StripeAccountId = company.StripeAccountId;
            existingCompany.TaxId = company.TaxId;
            existingCompany.IsActive = company.IsActive;
            existingCompany.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Updated rental company {CompanyId}", id);

            return Ok(existingCompany);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating rental company {CompanyId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Delete a rental company (soft delete by setting is_active to false)
    /// </summary>
    /// <param name="id">Company ID</param>
    /// <returns>No content</returns>
    [HttpDelete("{id}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    [ProducesResponseType(401)] // Unauthorized
    public async Task<IActionResult> DeleteRentalCompany(Guid id)
    {
        try
        {
            var company = await _context.RentalCompanies.FindAsync(id);
            if (company == null)
                return NotFound();

            // Soft delete by setting is_active to false
            company.IsActive = false;
            company.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Soft deleted rental company {CompanyId}", id);

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting rental company {CompanyId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Get company statistics
    /// </summary>
    /// <param name="id">Company ID</param>
    /// <returns>Company statistics</returns>
    [HttpGet("{id}/statistics")]
    [ProducesResponseType(typeof(object), 200)]
    [ProducesResponseType(404)]
    [ProducesResponseType(401)] // Unauthorized
    public async Task<IActionResult> GetCompanyStatistics(Guid id)
    {
        try
        {
            var company = await _context.RentalCompanies.FindAsync(id);
            if (company == null)
                return NotFound();

            var vehicleCount = await _context.Vehicles.CountAsync(v => v.CompanyId == id);
            var activeVehicleCount = await _context.Vehicles.CountAsync(v => v.CompanyId == id && v.Status == "available");
            var reservationCount = await _context.Reservations.CountAsync(r => r.CompanyId == id);
            var activeReservationCount = await _context.Reservations.CountAsync(r => r.CompanyId == id && r.Status == "confirmed");
            var totalRevenue = await _context.Payments
                .Where(p => p.CompanyId == id && p.Status == "succeeded")
                .SumAsync(p => p.Amount);

            return Ok(new
            {
                CompanyId = id,
                CompanyName = company.CompanyName,
                TotalVehicles = vehicleCount,
                ActiveVehicles = activeVehicleCount,
                TotalReservations = reservationCount,
                ActiveReservations = activeReservationCount,
                TotalRevenue = totalRevenue,
                GeneratedAt = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting company statistics for {CompanyId}", id);
            return StatusCode(500, "Internal server error");
        }
    }
}
