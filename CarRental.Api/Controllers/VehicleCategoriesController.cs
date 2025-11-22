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
public class VehicleCategoriesController : ControllerBase
{
    private readonly CarRentalDbContext _context;
    private readonly ILogger<VehicleCategoriesController> _logger;

    public VehicleCategoriesController(CarRentalDbContext context, ILogger<VehicleCategoriesController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Get all vehicle categories
    /// </summary>
    /// <param name="page">Page number</param>
    /// <param name="pageSize">Page size</param>
    /// <returns>List of vehicle categories</returns>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<VehicleCategory>), 200)]
    [ProducesResponseType(401)] // Unauthorized
    public async Task<IActionResult> GetVehicleCategories(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        try
        {
            var query = _context.VehicleCategories.AsQueryable();

            var totalCount = await query.CountAsync();

            var categories = await query
                .OrderBy(vc => vc.CategoryName)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return Ok(new
            {
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize,
                Categories = categories
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting vehicle categories");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Get a specific vehicle category by ID
    /// </summary>
    /// <param name="id">Category ID</param>
    /// <returns>Vehicle category details</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(VehicleCategory), 200)]
    [ProducesResponseType(404)]
    [ProducesResponseType(401)] // Unauthorized
    public async Task<IActionResult> GetVehicleCategory(Guid id)
    {
        try
        {
            var category = await _context.VehicleCategories
                .FirstOrDefaultAsync(vc => vc.CategoryId == id);

            if (category == null)
                return NotFound();

            return Ok(category);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting vehicle category {CategoryId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Create a new vehicle category
    /// </summary>
    /// <param name="category">Category details</param>
    /// <returns>Created category</returns>
    [HttpPost]
    [ProducesResponseType(typeof(VehicleCategory), 201)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)] // Unauthorized
    public async Task<IActionResult> CreateVehicleCategory([FromBody] VehicleCategory category)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Check if category name already exists
            if (await _context.VehicleCategories.AnyAsync(vc => vc.CategoryName == category.CategoryName))
            {
                return BadRequest("A category with this name already exists");
            }

            category.CategoryId = Guid.NewGuid();
            category.CreatedAt = DateTime.UtcNow;

            _context.VehicleCategories.Add(category);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Created vehicle category {CategoryId} with name {CategoryName}", 
                category.CategoryId, category.CategoryName);

            return CreatedAtAction(nameof(GetVehicleCategory), new { id = category.CategoryId }, category);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating vehicle category");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Update a vehicle category
    /// </summary>
    /// <param name="id">Category ID</param>
    /// <param name="category">Updated category details</param>
    /// <returns>Updated category</returns>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(VehicleCategory), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    [ProducesResponseType(401)] // Unauthorized
    public async Task<IActionResult> UpdateVehicleCategory(Guid id, [FromBody] VehicleCategory category)
    {
        try
        {
            if (id != category.CategoryId)
                return BadRequest("ID mismatch");

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var existingCategory = await _context.VehicleCategories.FindAsync(id);
            if (existingCategory == null)
                return NotFound();

            // Check if category name already exists for another category
            if (await _context.VehicleCategories.AnyAsync(vc => vc.CategoryName == category.CategoryName && vc.CategoryId != id))
            {
                return BadRequest("A category with this name already exists");
            }

            // Update properties
            existingCategory.CategoryName = category.CategoryName;
            existingCategory.Description = category.Description;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Updated vehicle category {CategoryId}", id);

            return Ok(existingCategory);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating vehicle category {CategoryId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Delete a vehicle category
    /// </summary>
    /// <param name="id">Category ID</param>
    /// <returns>No content</returns>
    [HttpDelete("{id}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)] // Unauthorized
    public async Task<IActionResult> DeleteVehicleCategory(Guid id)
    {
        try
        {
            var category = await _context.VehicleCategories.FindAsync(id);
            if (category == null)
                return NotFound();

            // Check if category is being used by any vehicles
            var vehiclesUsingCategory = await _context.Vehicles.AnyAsync(v => v.CategoryId == id);
            if (vehiclesUsingCategory)
            {
                return BadRequest("Cannot delete category that is being used by vehicles");
            }

            _context.VehicleCategories.Remove(category);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Deleted vehicle category {CategoryId}", id);

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting vehicle category {CategoryId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Get vehicles in a specific category
    /// </summary>
    /// <param name="id">Category ID</param>
    /// <param name="companyId">Filter by company ID</param>
    /// <param name="status">Filter by vehicle status</param>
    /// <param name="page">Page number</param>
    /// <param name="pageSize">Page size</param>
    /// <returns>Vehicles in the category</returns>
    [HttpGet("{id}/vehicles")]
    [ProducesResponseType(typeof(IEnumerable<Vehicle>), 200)]
    [ProducesResponseType(404)]
    [ProducesResponseType(401)] // Unauthorized
    public async Task<IActionResult> GetCategoryVehicles(
        Guid id,
        [FromQuery] Guid? companyId = null,
        [FromQuery] string? status = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        try
        {
            var category = await _context.VehicleCategories.FindAsync(id);
            if (category == null)
                return NotFound("Category not found");

            var query = _context.Vehicles
                .Include(v => v.Company)
                .Include(v => v.Category)
                .Where(v => v.CategoryId == id)
                .AsQueryable();

            if (companyId.HasValue)
                query = query.Where(v => v.CompanyId == companyId.Value);

            if (!string.IsNullOrEmpty(status))
                query = query.Where(v => v.Status == status);

            var totalCount = await query.CountAsync();

            var vehicles = await query
                .OrderBy(v => v.Make)
                .ThenBy(v => v.Model)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return Ok(new
            {
                CategoryId = id,
                CategoryName = category.CategoryName,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize,
                Vehicles = vehicles
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting vehicles for category {CategoryId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Get category statistics
    /// </summary>
    /// <param name="id">Category ID</param>
    /// <returns>Category statistics</returns>
    [HttpGet("{id}/statistics")]
    [ProducesResponseType(typeof(object), 200)]
    [ProducesResponseType(404)]
    [ProducesResponseType(401)] // Unauthorized
    public async Task<IActionResult> GetCategoryStatistics(Guid id)
    {
        try
        {
            var category = await _context.VehicleCategories.FindAsync(id);
            if (category == null)
                return NotFound("Category not found");

            var totalVehicles = await _context.Vehicles.CountAsync(v => v.CategoryId == id);
            var availableVehicles = await _context.Vehicles.CountAsync(v => v.CategoryId == id && v.Status == "available");
            var rentedVehicles = await _context.Vehicles.CountAsync(v => v.CategoryId == id && v.Status == "rented");
            var maintenanceVehicles = await _context.Vehicles.CountAsync(v => v.CategoryId == id && v.Status == "maintenance");

            var averageDailyRate = await _context.Vehicles
                .Where(v => v.CategoryId == id)
                .AverageAsync(v => v.DailyRate);

            return Ok(new
            {
                CategoryId = id,
                CategoryName = category.CategoryName,
                TotalVehicles = totalVehicles,
                AvailableVehicles = availableVehicles,
                RentedVehicles = rentedVehicles,
                MaintenanceVehicles = maintenanceVehicles,
                AverageDailyRate = Math.Round(averageDailyRate, 2),
                GeneratedAt = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting statistics for category {CategoryId}", id);
            return StatusCode(500, "Internal server error");
        }
    }
}
