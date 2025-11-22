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
public class CustomersController : ControllerBase
{
    private readonly CarRentalDbContext _context;
    private readonly ILogger<CustomersController> _logger;

    public CustomersController(CarRentalDbContext context, ILogger<CustomersController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Get all customers
    /// </summary>
    /// <param name="isVerified">Filter by verification status</param>
    /// <param name="search">Search by name or email</param>
    /// <param name="page">Page number</param>
    /// <param name="pageSize">Page size</param>
    /// <returns>List of customers</returns>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<Customer>), 200)]
    [ProducesResponseType(401)] // Unauthorized
    public async Task<IActionResult> GetCustomers(
        [FromQuery] bool? isVerified = null,
        [FromQuery] string? search = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        try
        {
            var query = _context.Customers.AsQueryable();

            if (isVerified.HasValue)
                query = query.Where(c => c.IsVerified == isVerified.Value);

            if (!string.IsNullOrEmpty(search))
            {
                var searchLower = search.ToLower();
                query = query.Where(c => 
                    c.FirstName.ToLower().Contains(searchLower) ||
                    c.LastName.ToLower().Contains(searchLower) ||
                    c.Email.ToLower().Contains(searchLower));
            }

            var totalCount = await query.CountAsync();

            var customers = await query
                .OrderBy(c => c.LastName)
                .ThenBy(c => c.FirstName)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return Ok(new
            {
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize,
                Customers = customers
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting customers");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Get a specific customer by ID
    /// </summary>
    /// <param name="id">Customer ID</param>
    /// <returns>Customer details</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(Customer), 200)]
    [ProducesResponseType(404)]
    [ProducesResponseType(401)] // Unauthorized
    public async Task<IActionResult> GetCustomer(Guid id)
    {
        try
        {
            var customer = await _context.Customers
                .FirstOrDefaultAsync(c => c.CustomerId == id);

            if (customer == null)
                return NotFound();

            return Ok(customer);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting customer {CustomerId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Create a new customer
    /// </summary>
    /// <param name="customer">Customer details</param>
    /// <returns>Created customer</returns>
    [HttpPost]
    [ProducesResponseType(typeof(Customer), 201)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)] // Unauthorized
    public async Task<IActionResult> CreateCustomer([FromBody] Customer customer)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Check if email already exists
            if (await _context.Customers.AnyAsync(c => c.Email == customer.Email))
            {
                return BadRequest("A customer with this email already exists");
            }

            customer.CustomerId = Guid.NewGuid();
            customer.CreatedAt = DateTime.UtcNow;
            customer.UpdatedAt = DateTime.UtcNow;

            _context.Customers.Add(customer);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Created customer {CustomerId} with email {Email}", 
                customer.CustomerId, customer.Email);

            return CreatedAtAction(nameof(GetCustomer), new { id = customer.CustomerId }, customer);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating customer");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Update a customer
    /// </summary>
    /// <param name="id">Customer ID</param>
    /// <param name="customer">Updated customer details</param>
    /// <returns>Updated customer</returns>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(Customer), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    [ProducesResponseType(401)] // Unauthorized
    public async Task<IActionResult> UpdateCustomer(Guid id, [FromBody] Customer customer)
    {
        try
        {
            if (id != customer.CustomerId)
                return BadRequest("ID mismatch");

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var existingCustomer = await _context.Customers.FindAsync(id);
            if (existingCustomer == null)
                return NotFound();

            // Check if email already exists for another customer
            if (await _context.Customers.AnyAsync(c => c.Email == customer.Email && c.CustomerId != id))
            {
                return BadRequest("A customer with this email already exists");
            }

            // Update properties
            existingCustomer.Email = customer.Email;
            existingCustomer.FirstName = customer.FirstName;
            existingCustomer.LastName = customer.LastName;
            existingCustomer.Phone = customer.Phone;
            existingCustomer.DateOfBirth = customer.DateOfBirth;
            existingCustomer.DriversLicenseNumber = customer.DriversLicenseNumber;
            existingCustomer.DriversLicenseState = customer.DriversLicenseState;
            existingCustomer.DriversLicenseExpiry = customer.DriversLicenseExpiry;
            existingCustomer.Address = customer.Address;
            existingCustomer.City = customer.City;
            existingCustomer.State = customer.State;
            existingCustomer.Country = customer.Country;
            existingCustomer.PostalCode = customer.PostalCode;
            existingCustomer.StripeCustomerId = customer.StripeCustomerId;
            existingCustomer.IsVerified = customer.IsVerified;
            existingCustomer.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Updated customer {CustomerId}", id);

            return Ok(existingCustomer);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating customer {CustomerId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Delete a customer
    /// </summary>
    /// <param name="id">Customer ID</param>
    /// <returns>No content</returns>
    [HttpDelete("{id}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    [ProducesResponseType(401)] // Unauthorized
    public async Task<IActionResult> DeleteCustomer(Guid id)
    {
        try
        {
            var customer = await _context.Customers.FindAsync(id);
            if (customer == null)
                return NotFound();

            _context.Customers.Remove(customer);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Deleted customer {CustomerId}", id);

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting customer {CustomerId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Get customer's reservations
    /// </summary>
    /// <param name="id">Customer ID</param>
    /// <param name="status">Filter by reservation status</param>
    /// <param name="page">Page number</param>
    /// <param name="pageSize">Page size</param>
    /// <returns>Customer's reservations</returns>
    [HttpGet("{id}/reservations")]
    [ProducesResponseType(typeof(IEnumerable<Reservation>), 200)]
    [ProducesResponseType(404)]
    [ProducesResponseType(401)] // Unauthorized
    public async Task<IActionResult> GetCustomerReservations(
        Guid id,
        [FromQuery] string? status = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        try
        {
            var customer = await _context.Customers.FindAsync(id);
            if (customer == null)
                return NotFound();

            var query = _context.Reservations
                .Include(r => r.Vehicle)
                .Include(r => r.Company)
                .Where(r => r.CustomerId == id)
                .AsQueryable();

            if (!string.IsNullOrEmpty(status))
                query = query.Where(r => r.Status == status);

            var totalCount = await query.CountAsync();

            var reservations = await query
                .OrderByDescending(r => r.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return Ok(new
            {
                CustomerId = id,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize,
                Reservations = reservations
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting reservations for customer {CustomerId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Get customer's payment methods
    /// </summary>
    /// <param name="id">Customer ID</param>
    /// <returns>Customer's payment methods</returns>
    [HttpGet("{id}/payment-methods")]
    [ProducesResponseType(typeof(IEnumerable<CustomerPaymentMethod>), 200)]
    [ProducesResponseType(404)]
    [ProducesResponseType(401)] // Unauthorized
    public async Task<IActionResult> GetCustomerPaymentMethods(Guid id)
    {
        try
        {
            var customer = await _context.Customers.FindAsync(id);
            if (customer == null)
                return NotFound();

            var paymentMethods = await _context.CustomerPaymentMethods
                .Where(pm => pm.CustomerId == id)
                .OrderBy(pm => pm.IsDefault ? 0 : 1)
                .ThenBy(pm => pm.CreatedAt)
                .ToListAsync();

            return Ok(paymentMethods);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting payment methods for customer {CustomerId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Verify a customer
    /// </summary>
    /// <param name="id">Customer ID</param>
    /// <returns>Updated customer</returns>
    [HttpPost("{id}/verify")]
    [ProducesResponseType(typeof(Customer), 200)]
    [ProducesResponseType(404)]
    [ProducesResponseType(401)] // Unauthorized
    public async Task<IActionResult> VerifyCustomer(Guid id)
    {
        try
        {
            var customer = await _context.Customers.FindAsync(id);
            if (customer == null)
                return NotFound();

            customer.IsVerified = true;
            customer.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Verified customer {CustomerId}", id);

            return Ok(customer);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verifying customer {CustomerId}", id);
            return StatusCode(500, "Internal server error");
        }
    }
}
