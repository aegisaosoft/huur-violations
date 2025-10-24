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

using System.Text.Json.Serialization;

namespace USAHuurViolations.Models
{
    public class RmcPayOperatorInfoResponse
    {
        [JsonPropertyName("status")]
        public int Status { get; set; }

        [JsonPropertyName("data")]
        public OperatorData Data { get; set; } = new();

        [JsonPropertyName("message")]
        public string Message { get; set; } = string.Empty;
    }

    public class OperatorData
    {
        [JsonPropertyName("operators")]
        public List<Operator> Operators { get; set; } = new();

        [JsonPropertyName("search_type")]
        public int SearchType { get; set; }
    }

    public class Operator
    {
        [JsonPropertyName("ticketnumber")]
        public string TicketNumber { get; set; } = string.Empty;

        [JsonPropertyName("datecreated")]
        public string DateCreated { get; set; } = string.Empty;

        [JsonPropertyName("lpn")]
        public string Lpn { get; set; } = string.Empty;

        [JsonPropertyName("vin")]
        public string? Vin { get; set; }

        [JsonPropertyName("operator_name")]
        public string OperatorName { get; set; } = string.Empty;

        [JsonPropertyName("operator_id")]
        public string OperatorId { get; set; } = string.Empty;

        [JsonPropertyName("subdomain")]
        public string Subdomain { get; set; } = string.Empty;

        [JsonPropertyName("operator_location")]
        public string? OperatorLocation { get; set; }

        [JsonPropertyName("redirect_url")]
        public string RedirectUrl { get; set; } = string.Empty;

        [JsonPropertyName("immobilization_device_number")]
        public string? ImmobilizationDeviceNumber { get; set; }

        [JsonPropertyName("immobilization_status")]
        public string? ImmobilizationStatus { get; set; }

        [JsonPropertyName("immobilization_id")]
        public string? ImmobilizationId { get; set; }
    }
}
