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

namespace HuurViolations.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ViolationsController : ControllerBase
{
    private readonly ILogger<ViolationsController> _logger;

    public ViolationsController(ILogger<ViolationsController> logger)
    {
        _logger = logger;
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
        
        // Sample data - replace with actual data access
        var violations = new[]
        {
            new { Id = 1, Description = "Late rent payment", Amount = 50.00m, Date = DateTime.Now.AddDays(-5) },
            new { Id = 2, Description = "Property damage", Amount = 200.00m, Date = DateTime.Now.AddDays(-10) },
            new { Id = 3, Description = "Noise complaint", Amount = 25.00m, Date = DateTime.Now.AddDays(-2) }
        };

        return Ok(violations);
    }

    [HttpGet("{id}")]
    public IActionResult GetViolation(int id)
    {
        _logger.LogInformation("Getting violation with ID: {Id}", id);
        
        // Sample data - replace with actual data access
        var violation = new { Id = id, Description = "Sample violation", Amount = 100.00m, Date = DateTime.Now };
        
        return Ok(violation);
    }

    [HttpPost]
    public IActionResult CreateViolation([FromBody] CreateViolationRequest request)
    {
        _logger.LogInformation("Creating new violation: {Description}", request.Description);
        
        // Sample response - replace with actual data creation
        var violation = new { Id = 4, Description = request.Description, Amount = request.Amount, Date = DateTime.Now };
        
        return CreatedAtAction(nameof(GetViolation), new { id = violation.Id }, violation);
    }

    [HttpPut("{id}")]
    public IActionResult UpdateViolation(int id, [FromBody] UpdateViolationRequest request)
    {
        _logger.LogInformation("Updating violation with ID: {Id}", id);
        
        // Sample response - replace with actual data update
        var violation = new { Id = id, Description = request.Description, Amount = request.Amount, Date = DateTime.Now };
        
        return Ok(violation);
    }

    [HttpDelete("{id}")]
    public IActionResult DeleteViolation(int id)
    {
        _logger.LogInformation("Deleting violation with ID: {Id}", id);
        
        return NoContent();
    }

    /// <summary>
    /// Get all violations with JSON parameters (public endpoint)
    /// </summary>
    /// <param name="request">Search parameters</param>
    /// <returns>List of violations based on parameters</returns>
    [HttpPost("get-all")]
    [ProducesResponseType(typeof(IEnumerable<object>), 200)]
    public IActionResult GetAllViolations([FromBody] ViolationSearchRequest request)
    {
        _logger.LogInformation("Getting all violations with parameters: {Parameters}", request);
        
        // Sample data - replace with actual data access based on parameters
        var violations = new[]
        {
            new { 
                Id = 1, 
                Description = "Late rent payment", 
                Amount = 50.00m, 
                Date = DateTime.Now.AddDays(-5),
                Finder = "Blinkay",
                State = "FL",
                LicensePlate = "ABC123"
            },
            new { 
                Id = 2, 
                Description = "Property damage", 
                Amount = 200.00m, 
                Date = DateTime.Now.AddDays(-10),
                Finder = "Tampa",
                State = "FL",
                LicensePlate = "XYZ789"
            },
            new { 
                Id = 3, 
                Description = "Noise complaint", 
                Amount = 25.00m, 
                Date = DateTime.Now.AddDays(-2),
                Finder = "Metropolis",
                State = "FL",
                LicensePlate = "DEF456"
            }
        };

        // Filter based on parameters if provided
        var filteredViolations = violations.AsEnumerable();
        
        // Filter by date_from if provided
        if (!string.IsNullOrEmpty(request.date_from))
        {
            if (DateTime.TryParse(request.date_from, out var fromDate))
            {
                filteredViolations = filteredViolations.Where(v => v.Date >= fromDate);
            }
        }
        
        // Filter by Finders array if provided
        if (request.Finders != null && request.Finders.Length > 0)
        {
            filteredViolations = filteredViolations.Where(v => 
                request.Finders.Any(f => v.Finder.Equals(f, StringComparison.OrdinalIgnoreCase)));
        }

        return Ok(new
        {
            TotalCount = filteredViolations.Count(),
            Parameters = request,
            Violations = filteredViolations.ToArray()
        });
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
}

public class CreateViolationRequest
{
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
}

public class UpdateViolationRequest
{
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
}

public class ViolationSearchRequest
{
    /// <summary>
    /// Date from in YYYY-MM-DD format
    /// </summary>
    public string? date_from { get; set; }
    
    /// <summary>
    /// Array of finders to search
    /// </summary>
    public string[]? Finders { get; set; }
}


