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
using System;

namespace HuurVioaltionsAPI
{
    public class ViolationResult
    {
        public string? CitationNumber { get; set; }
        public string? Agency { get; set; }
        public string? ViolationCode { get; set; }
        public string? Description { get; set; }
        public decimal Amount { get; set; }
        public DateTime IssueDate { get; set; }
        public string? Status { get; set; }
        public string? LicensePlate { get; set; }
        public string? State { get; set; }
    }
}


