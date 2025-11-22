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
public class PaymentsController : ControllerBase
{
    private readonly CarRentalDbContext _context;
    private readonly ILogger<PaymentsController> _logger;

    public PaymentsController(CarRentalDbContext context, ILogger<PaymentsController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Get all payments
    /// </summary>
    /// <param name="customerId">Filter by customer ID</param>
    /// <param name="companyId">Filter by company ID</param>
    /// <param name="status">Filter by payment status</param>
    /// <param name="paymentType">Filter by payment type</param>
    /// <param name="page">Page number</param>
    /// <param name="pageSize">Page size</param>
    /// <returns>List of payments</returns>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<Payment>), 200)]
    [ProducesResponseType(401)] // Unauthorized
    public async Task<IActionResult> GetPayments(
        [FromQuery] Guid? customerId = null,
        [FromQuery] Guid? companyId = null,
        [FromQuery] string? status = null,
        [FromQuery] string? paymentType = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        try
        {
            var query = _context.Payments
                .Include(p => p.Customer)
                .Include(p => p.Company)
                .Include(p => p.Reservation)
                .Include(p => p.Rental)
                .AsQueryable();

            if (customerId.HasValue)
                query = query.Where(p => p.CustomerId == customerId.Value);

            if (companyId.HasValue)
                query = query.Where(p => p.CompanyId == companyId.Value);

            if (!string.IsNullOrEmpty(status))
                query = query.Where(p => p.Status == status);

            if (!string.IsNullOrEmpty(paymentType))
                query = query.Where(p => p.PaymentType == paymentType);

            var totalCount = await query.CountAsync();

            var payments = await query
                .OrderByDescending(p => p.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return Ok(new
            {
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize,
                Payments = payments
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting payments");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Get a specific payment by ID
    /// </summary>
    /// <param name="id">Payment ID</param>
    /// <returns>Payment details</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(Payment), 200)]
    [ProducesResponseType(404)]
    [ProducesResponseType(401)] // Unauthorized
    public async Task<IActionResult> GetPayment(Guid id)
    {
        try
        {
            var payment = await _context.Payments
                .Include(p => p.Customer)
                .Include(p => p.Company)
                .Include(p => p.Reservation)
                .Include(p => p.Rental)
                .FirstOrDefaultAsync(p => p.PaymentId == id);

            if (payment == null)
                return NotFound();

            return Ok(payment);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting payment {PaymentId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Create a new payment
    /// </summary>
    /// <param name="payment">Payment details</param>
    /// <returns>Created payment</returns>
    [HttpPost]
    [ProducesResponseType(typeof(Payment), 201)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)] // Unauthorized
    public async Task<IActionResult> CreatePayment([FromBody] Payment payment)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Validate that customer exists
            var customer = await _context.Customers.FindAsync(payment.CustomerId);
            if (customer == null)
                return BadRequest("Customer not found");

            // Validate that company exists
            var company = await _context.RentalCompanies.FindAsync(payment.CompanyId);
            if (company == null)
                return BadRequest("Company not found");

            // Validate reservation if provided
            if (payment.ReservationId.HasValue)
            {
                var reservation = await _context.Reservations.FindAsync(payment.ReservationId.Value);
                if (reservation == null)
                    return BadRequest("Reservation not found");
            }

            // Validate rental if provided
            if (payment.RentalId.HasValue)
            {
                var rental = await _context.Rentals.FindAsync(payment.RentalId.Value);
                if (rental == null)
                    return BadRequest("Rental not found");
            }

            payment.PaymentId = Guid.NewGuid();
            payment.CreatedAt = DateTime.UtcNow;

            _context.Payments.Add(payment);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Created payment {PaymentId} for customer {CustomerId}", 
                payment.PaymentId, payment.CustomerId);

            return CreatedAtAction(nameof(GetPayment), new { id = payment.PaymentId }, payment);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating payment");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Update a payment
    /// </summary>
    /// <param name="id">Payment ID</param>
    /// <param name="payment">Updated payment details</param>
    /// <returns>Updated payment</returns>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(Payment), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    [ProducesResponseType(401)] // Unauthorized
    public async Task<IActionResult> UpdatePayment(Guid id, [FromBody] Payment payment)
    {
        try
        {
            if (id != payment.PaymentId)
                return BadRequest("ID mismatch");

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var existingPayment = await _context.Payments.FindAsync(id);
            if (existingPayment == null)
                return NotFound();

            // Update properties
            existingPayment.Amount = payment.Amount;
            existingPayment.Currency = payment.Currency;
            existingPayment.PaymentType = payment.PaymentType;
            existingPayment.PaymentMethod = payment.PaymentMethod;
            existingPayment.StripePaymentIntentId = payment.StripePaymentIntentId;
            existingPayment.StripeChargeId = payment.StripeChargeId;
            existingPayment.StripePaymentMethodId = payment.StripePaymentMethodId;
            existingPayment.Status = payment.Status;
            existingPayment.FailureReason = payment.FailureReason;
            existingPayment.ProcessedAt = payment.ProcessedAt;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Updated payment {PaymentId}", id);

            return Ok(existingPayment);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating payment {PaymentId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Delete a payment
    /// </summary>
    /// <param name="id">Payment ID</param>
    /// <returns>No content</returns>
    [HttpDelete("{id}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    [ProducesResponseType(401)] // Unauthorized
    public async Task<IActionResult> DeletePayment(Guid id)
    {
        try
        {
            var payment = await _context.Payments.FindAsync(id);
            if (payment == null)
                return NotFound();

            _context.Payments.Remove(payment);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Deleted payment {PaymentId}", id);

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting payment {PaymentId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Process a payment (update status to processing)
    /// </summary>
    /// <param name="id">Payment ID</param>
    /// <returns>Updated payment</returns>
    [HttpPost("{id}/process")]
    [ProducesResponseType(typeof(Payment), 200)]
    [ProducesResponseType(404)]
    [ProducesResponseType(401)] // Unauthorized
    public async Task<IActionResult> ProcessPayment(Guid id)
    {
        try
        {
            var payment = await _context.Payments.FindAsync(id);
            if (payment == null)
                return NotFound();

            payment.Status = "processing";
            payment.ProcessedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Processing payment {PaymentId}", id);

            return Ok(payment);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing payment {PaymentId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Mark payment as succeeded
    /// </summary>
    /// <param name="id">Payment ID</param>
    /// <returns>Updated payment</returns>
    [HttpPost("{id}/succeed")]
    [ProducesResponseType(typeof(Payment), 200)]
    [ProducesResponseType(404)]
    [ProducesResponseType(401)] // Unauthorized
    public async Task<IActionResult> SucceedPayment(Guid id)
    {
        try
        {
            var payment = await _context.Payments.FindAsync(id);
            if (payment == null)
                return NotFound();

            payment.Status = "succeeded";
            payment.ProcessedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Payment {PaymentId} succeeded", id);

            return Ok(payment);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking payment {PaymentId} as succeeded", id);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Mark payment as failed
    /// </summary>
    /// <param name="id">Payment ID</param>
    /// <param name="failureReason">Reason for failure</param>
    /// <returns>Updated payment</returns>
    [HttpPost("{id}/fail")]
    [ProducesResponseType(typeof(Payment), 200)]
    [ProducesResponseType(404)]
    [ProducesResponseType(401)] // Unauthorized
    public async Task<IActionResult> FailPayment(Guid id, [FromBody] string? failureReason = null)
    {
        try
        {
            var payment = await _context.Payments.FindAsync(id);
            if (payment == null)
                return NotFound();

            payment.Status = "failed";
            payment.FailureReason = failureReason;
            payment.ProcessedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Payment {PaymentId} failed: {FailureReason}", id, failureReason);

            return Ok(payment);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking payment {PaymentId} as failed", id);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Get payment statistics
    /// </summary>
    /// <param name="companyId">Company ID for statistics</param>
    /// <param name="startDate">Start date for statistics</param>
    /// <param name="endDate">End date for statistics</param>
    /// <returns>Payment statistics</returns>
    [HttpGet("statistics")]
    [ProducesResponseType(typeof(object), 200)]
    [ProducesResponseType(401)] // Unauthorized
    public async Task<IActionResult> GetPaymentStatistics(
        [FromQuery] Guid? companyId = null,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        try
        {
            var query = _context.Payments.AsQueryable();

            if (companyId.HasValue)
                query = query.Where(p => p.CompanyId == companyId.Value);

            if (startDate.HasValue)
                query = query.Where(p => p.CreatedAt >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(p => p.CreatedAt <= endDate.Value);

            var totalPayments = await query.CountAsync();
            var succeededPayments = await query.CountAsync(p => p.Status == "succeeded");
            var failedPayments = await query.CountAsync(p => p.Status == "failed");
            var pendingPayments = await query.CountAsync(p => p.Status == "pending");
            var totalAmount = await query.Where(p => p.Status == "succeeded").SumAsync(p => p.Amount);
            var averageAmount = await query.Where(p => p.Status == "succeeded").AverageAsync(p => p.Amount);

            return Ok(new
            {
                TotalPayments = totalPayments,
                SucceededPayments = succeededPayments,
                FailedPayments = failedPayments,
                PendingPayments = pendingPayments,
                TotalAmount = totalAmount,
                AverageAmount = averageAmount,
                SuccessRate = totalPayments > 0 ? (double)succeededPayments / totalPayments * 100 : 0,
                GeneratedAt = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting payment statistics");
            return StatusCode(500, "Internal server error");
        }
    }
}
