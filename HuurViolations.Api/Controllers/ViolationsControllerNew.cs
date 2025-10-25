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
using HuurViolations.Api.Services;

namespace HuurViolations.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ViolationsControllerNew : ControllerBase
{
    private readonly ILogger<ViolationsControllerNew> _logger;
    private readonly IHuurApiTokenValidator _huurApiTokenValidator;

    public ViolationsControllerNew(ILogger<ViolationsControllerNew> logger, IHuurApiTokenValidator huurApiTokenValidator)
    {
        _logger = logger;
        _huurApiTokenValidator = huurApiTokenValidator;
    }

    /// <summary>
    /// Get all violations (public endpoint)
    /// </summary>
    /// <returns>List of violations</returns>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<object>), 200)]
    public IActionResult GetViolations()
    {
        _logger.LogInformation("Getting all violations");
        
        // Sample data - replace with actual data retrieval
        var violations = new[]
        {
            new { Id = 1, Description = "Parking Violation", Amount = 50.00, Date = DateTime.Now.AddDays(-1) },
            new { Id = 2, Description = "Speeding Ticket", Amount = 100.00, Date = DateTime.Now.AddDays(-2) },
            new { Id = 3, Description = "Red Light Violation", Amount = 150.00, Date = DateTime.Now.AddDays(-3) }
        };

        return Ok(violations);
    }

    /// <summary>
    /// Get violation by ID (public endpoint)
    /// </summary>
    /// <param name="id">Violation ID</param>
    /// <returns>Violation details</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(object), 200)]
    [ProducesResponseType(404)]
    public IActionResult GetViolation(int id)
    {
        _logger.LogInformation("Getting violation with ID: {Id}", id);
        
        // Sample data - replace with actual data retrieval
        var violation = new { Id = id, Description = "Parking Violation", Amount = 50.00, Date = DateTime.Now.AddDays(-1) };
        
        return Ok(violation);
    }

    /// <summary>
    /// Create new violation (public endpoint)
    /// </summary>
    /// <param name="violation">Violation data</param>
    /// <returns>Created violation</returns>
    [HttpPost]
    [ProducesResponseType(typeof(object), 201)]
    [ProducesResponseType(400)]
    public IActionResult CreateViolation([FromBody] object violation)
    {
        _logger.LogInformation("Creating new violation");
        
        // Sample response - replace with actual data creation
        var createdViolation = new { Id = 4, Description = "New Violation", Amount = 75.00, Date = DateTime.Now };
        
        return CreatedAtAction(nameof(GetViolation), new { id = 4 }, createdViolation);
    }

    /// <summary>
    /// Update violation (public endpoint)
    /// </summary>
    /// <param name="id">Violation ID</param>
    /// <param name="request">Update data</param>
    /// <returns>Updated violation</returns>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(object), 200)]
    [ProducesResponseType(404)]
    public IActionResult UpdateViolation(int id, [FromBody] object request)
    {
        _logger.LogInformation("Updating violation with ID: {Id}", id);
        
        // Sample response - replace with actual data update
        var violation = new { Id = id, Description = "Updated Violation", Amount = 100.00, Date = DateTime.Now };
        
        return Ok(violation);
    }

    /// <summary>
    /// Delete violation (public endpoint)
    /// </summary>
    /// <param name="id">Violation ID</param>
    /// <returns>No content</returns>
    [HttpDelete("{id}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    public IActionResult DeleteViolation(int id)
    {
        _logger.LogInformation("Deleting violation with ID: {Id}", id);
        
        return NoContent();
    }

    /// <summary>
    /// Health check endpoint (public)
    /// </summary>
    /// <returns>API status</returns>
    [HttpGet("health")]
    [ProducesResponseType(typeof(object), 200)]
    public IActionResult GetHealth()
    {
        return Ok(new 
        { 
            Status = "Healthy", 
            Timestamp = DateTime.UtcNow,
            Version = "1.0.0",
            Message = "HuurViolations API is running"
        });
    }

    /// <summary>
    /// Get all violations with JSON parameters (requires HuurApi Bearer authentication)
    /// </summary>
    /// <param name="request">Search parameters</param>
    /// <returns>List of violations based on parameters</returns>
    [HttpPost("get-all")]
    [Authorize] // Requires Bearer token authentication
    [ProducesResponseType(typeof(IEnumerable<object>), 200)]
    [ProducesResponseType(401)] // Unauthorized
    public async Task<IActionResult> GetAllViolations([FromBody] ViolationSearchRequest request)
    {
        _logger.LogInformation("Getting all violations with parameters: {Parameters}", request);
        
        try
        {
            // Get the Bearer token from the Authorization header
            var authHeader = Request.Headers["Authorization"].FirstOrDefault();
            if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
            {
                _logger.LogWarning("No valid Bearer token provided");
                return Unauthorized(new { error = "Invalid or missing Bearer token" });
            }

            var token = authHeader.Substring("Bearer ".Length).Trim();
            _logger.LogInformation("Using HuurApi token validation");

            // Validate token with HuurApi
            var isValidToken = await _huurApiTokenValidator.ValidateTokenAsync(token);
            
            if (!isValidToken)
            {
                _logger.LogWarning("HuurApi token validation failed");
                return Unauthorized(new { error = "Token validation failed", details = "Invalid or expired token" });
            }

            _logger.LogInformation("HuurApi token validated successfully, returning empty array as requested");
            
            // Return empty array as requested when token is valid
            return Ok(new
            {
                TotalCount = 0,
                Parameters = request,
                Violations = new object[0],
                Message = "HuurApi token validated successfully"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during HuurApi token validation");
            return StatusCode(500, new { error = "Internal server error during authentication" });
        }
    }
}

public class ViolationSearchRequest
{
    public string DateFrom { get; set; } = string.Empty;
    public Finder[] Finders { get; set; } = Array.Empty<Finder>();
}

public class Finder
{
    public string FinderName { get; set; } = string.Empty;
}
