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

namespace HuurViolations.Api.Models;

public class ViolationDto
{
    public string CitationNumber { get; set; } = string.Empty;
    public string Agency { get; set; } = string.Empty;
    public string LicensePlate { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public DateTime IssueDate { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "USD";
    public string PaymentStatus { get; set; } = string.Empty;
    public string FineType { get; set; } = string.Empty;
    public string Link { get; set; } = string.Empty;
}


