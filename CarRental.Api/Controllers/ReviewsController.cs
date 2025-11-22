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
public class ReviewsController : ControllerBase
{
    private readonly CarRentalDbContext _context;
    private readonly ILogger<ReviewsController> _logger;

    public ReviewsController(CarRentalDbContext context, ILogger<ReviewsController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Get all reviews
    /// </summary>
    /// <param name="rentalId">Filter by rental ID</param>
    /// <param name="customerId">Filter by customer ID</param>
    /// <param name="companyId">Filter by company ID</param>
    /// <param name="vehicleId">Filter by vehicle ID</param>
    /// <param name="minRating">Filter by minimum rating</param>
    /// <param name="page">Page number</param>
    /// <param name="pageSize">Page size</param>
    /// <returns>List of reviews</returns>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<Review>), 200)]
    [ProducesResponseType(401)] // Unauthorized
    public async Task<IActionResult> GetReviews(
        [FromQuery] Guid? rentalId = null,
        [FromQuery] Guid? customerId = null,
        [FromQuery] Guid? companyId = null,
        [FromQuery] Guid? vehicleId = null,
        [FromQuery] int? minRating = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        try
        {
            var query = _context.Reviews
                .Include(r => r.Customer)
                .Include(r => r.Company)
                .Include(r => r.Vehicle)
                .Include(r => r.Rental)
                .AsQueryable();

            if (rentalId.HasValue)
                query = query.Where(r => r.RentalId == rentalId.Value);

            if (customerId.HasValue)
                query = query.Where(r => r.CustomerId == customerId.Value);

            if (companyId.HasValue)
                query = query.Where(r => r.CompanyId == companyId.Value);

            if (vehicleId.HasValue)
                query = query.Where(r => r.VehicleId == vehicleId.Value);

            if (minRating.HasValue)
                query = query.Where(r => r.Rating >= minRating.Value);

            var totalCount = await query.CountAsync();

            var reviews = await query
                .OrderByDescending(r => r.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return Ok(new
            {
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize,
                Reviews = reviews
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting reviews");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Get a specific review by ID
    /// </summary>
    /// <param name="id">Review ID</param>
    /// <returns>Review details</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(Review), 200)]
    [ProducesResponseType(404)]
    [ProducesResponseType(401)] // Unauthorized
    public async Task<IActionResult> GetReview(Guid id)
    {
        try
        {
            var review = await _context.Reviews
                .Include(r => r.Customer)
                .Include(r => r.Company)
                .Include(r => r.Vehicle)
                .Include(r => r.Rental)
                .FirstOrDefaultAsync(r => r.ReviewId == id);

            if (review == null)
                return NotFound();

            return Ok(review);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting review {ReviewId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Create a new review
    /// </summary>
    /// <param name="review">Review details</param>
    /// <returns>Created review</returns>
    [HttpPost]
    [ProducesResponseType(typeof(Review), 201)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)] // Unauthorized
    public async Task<IActionResult> CreateReview([FromBody] Review review)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Validate that rental exists and is completed
            var rental = await _context.Rentals.FindAsync(review.RentalId);
            if (rental == null)
                return BadRequest("Rental not found");

            if (rental.Status != "completed")
                return BadRequest("Rental must be completed to leave a review");

            // Validate that customer exists
            var customer = await _context.Customers.FindAsync(review.CustomerId);
            if (customer == null)
                return BadRequest("Customer not found");

            // Validate that company exists
            var company = await _context.RentalCompanies.FindAsync(review.CompanyId);
            if (company == null)
                return BadRequest("Company not found");

            // Validate that vehicle exists
            var vehicle = await _context.Vehicles.FindAsync(review.VehicleId);
            if (vehicle == null)
                return BadRequest("Vehicle not found");

            // Check if customer has already reviewed this rental
            var existingReview = await _context.Reviews
                .FirstOrDefaultAsync(r => r.RentalId == review.RentalId && r.CustomerId == review.CustomerId);
            
            if (existingReview != null)
                return BadRequest("Customer has already reviewed this rental");

            // Validate rating
            if (review.Rating < 1 || review.Rating > 5)
                return BadRequest("Rating must be between 1 and 5");

            review.ReviewId = Guid.NewGuid();
            review.CreatedAt = DateTime.UtcNow;

            _context.Reviews.Add(review);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Created review {ReviewId} for rental {RentalId}", 
                review.ReviewId, review.RentalId);

            return CreatedAtAction(nameof(GetReview), new { id = review.ReviewId }, review);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating review");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Update a review
    /// </summary>
    /// <param name="id">Review ID</param>
    /// <param name="review">Updated review details</param>
    /// <returns>Updated review</returns>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(Review), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    [ProducesResponseType(401)] // Unauthorized
    public async Task<IActionResult> UpdateReview(Guid id, [FromBody] Review review)
    {
        try
        {
            if (id != review.ReviewId)
                return BadRequest("ID mismatch");

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var existingReview = await _context.Reviews.FindAsync(id);
            if (existingReview == null)
                return NotFound();

            // Validate rating
            if (review.Rating < 1 || review.Rating > 5)
                return BadRequest("Rating must be between 1 and 5");

            // Update properties
            existingReview.Rating = review.Rating;
            existingReview.Comment = review.Comment;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Updated review {ReviewId}", id);

            return Ok(existingReview);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating review {ReviewId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Delete a review
    /// </summary>
    /// <param name="id">Review ID</param>
    /// <returns>No content</returns>
    [HttpDelete("{id}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    [ProducesResponseType(401)] // Unauthorized
    public async Task<IActionResult> DeleteReview(Guid id)
    {
        try
        {
            var review = await _context.Reviews.FindAsync(id);
            if (review == null)
                return NotFound();

            _context.Reviews.Remove(review);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Deleted review {ReviewId}", id);

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting review {ReviewId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Get reviews for a specific vehicle
    /// </summary>
    /// <param name="vehicleId">Vehicle ID</param>
    /// <param name="page">Page number</param>
    /// <param name="pageSize">Page size</param>
    /// <returns>Vehicle reviews</returns>
    [HttpGet("vehicle/{vehicleId}")]
    [ProducesResponseType(typeof(IEnumerable<Review>), 200)]
    [ProducesResponseType(401)] // Unauthorized
    public async Task<IActionResult> GetVehicleReviews(
        Guid vehicleId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        try
        {
            var vehicle = await _context.Vehicles.FindAsync(vehicleId);
            if (vehicle == null)
                return NotFound("Vehicle not found");

            var query = _context.Reviews
                .Include(r => r.Customer)
                .Where(r => r.VehicleId == vehicleId)
                .AsQueryable();

            var totalCount = await query.CountAsync();

            var reviews = await query
                .OrderByDescending(r => r.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return Ok(new
            {
                VehicleId = vehicleId,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize,
                Reviews = reviews
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting reviews for vehicle {VehicleId}", vehicleId);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Get reviews for a specific company
    /// </summary>
    /// <param name="companyId">Company ID</param>
    /// <param name="page">Page number</param>
    /// <param name="pageSize">Page size</param>
    /// <returns>Company reviews</returns>
    [HttpGet("company/{companyId}")]
    [ProducesResponseType(typeof(IEnumerable<Review>), 200)]
    [ProducesResponseType(401)] // Unauthorized
    public async Task<IActionResult> GetCompanyReviews(
        Guid companyId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        try
        {
            var company = await _context.RentalCompanies.FindAsync(companyId);
            if (company == null)
                return NotFound("Company not found");

            var query = _context.Reviews
                .Include(r => r.Customer)
                .Include(r => r.Vehicle)
                .Where(r => r.CompanyId == companyId)
                .AsQueryable();

            var totalCount = await query.CountAsync();

            var reviews = await query
                .OrderByDescending(r => r.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return Ok(new
            {
                CompanyId = companyId,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize,
                Reviews = reviews
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting reviews for company {CompanyId}", companyId);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Get review statistics
    /// </summary>
    /// <param name="companyId">Company ID for statistics</param>
    /// <param name="vehicleId">Vehicle ID for statistics</param>
    /// <returns>Review statistics</returns>
    [HttpGet("statistics")]
    [ProducesResponseType(typeof(object), 200)]
    [ProducesResponseType(401)] // Unauthorized
    public async Task<IActionResult> GetReviewStatistics(
        [FromQuery] Guid? companyId = null,
        [FromQuery] Guid? vehicleId = null)
    {
        try
        {
            var query = _context.Reviews.AsQueryable();

            if (companyId.HasValue)
                query = query.Where(r => r.CompanyId == companyId.Value);

            if (vehicleId.HasValue)
                query = query.Where(r => r.VehicleId == vehicleId.Value);

            var totalReviews = await query.CountAsync();
            var averageRating = await query.AverageAsync(r => (double)r.Rating);
            var ratingDistribution = await query
                .GroupBy(r => r.Rating)
                .Select(g => new { Rating = g.Key, Count = g.Count() })
                .OrderBy(x => x.Rating)
                .ToListAsync();

            return Ok(new
            {
                TotalReviews = totalReviews,
                AverageRating = Math.Round(averageRating, 2),
                RatingDistribution = ratingDistribution,
                GeneratedAt = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting review statistics");
            return StatusCode(500, "Internal server error");
        }
    }
}
