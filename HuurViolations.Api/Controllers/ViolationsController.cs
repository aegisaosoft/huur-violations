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
using HuurViolations.Auth;

namespace HuurViolations.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ViolationsController : ControllerBase
{
    private readonly ILogger<ViolationsController> _logger;
    private readonly IAuthenticationService _authenticationService;

    public ViolationsController(ILogger<ViolationsController> logger, IAuthenticationService authenticationService)
    {
        _logger = logger;
        _authenticationService = authenticationService;
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
    /// Get all violations with JSON parameters (requires Bearer authentication)
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
            _logger.LogInformation("Using authentication service to validate token");

            // Use the authentication service to validate the token
            var authResult = await _authenticationService.ValidateTokenAsync(token);
            
            if (!authResult.IsValid)
            {
                _logger.LogWarning("Token validation failed: {Error}", authResult.Error);
                return Unauthorized(new { error = "Token validation failed", details = authResult.Error });
            }

            _logger.LogInformation("Token validated successfully, returning empty array as requested");
            
            // Return empty array as requested when token is valid
            return Ok(new
            {
                TotalCount = 0,
                Parameters = request,
                Violations = new object[0],
                Message = "Token validated successfully"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during token validation");
            return StatusCode(500, new { error = "Internal server error during authentication" });
        }
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
    public FinderItem[]? Finders { get; set; }
}

public class FinderItem
{
    /// <summary>
    /// Name of the finder
    /// </summary>
    public string finder_name { get; set; } = string.Empty;
}



