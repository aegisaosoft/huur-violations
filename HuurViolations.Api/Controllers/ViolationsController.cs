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

    [HttpGet]
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


