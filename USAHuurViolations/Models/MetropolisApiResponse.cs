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
    public class MetropolisApiResponse
    {
        [JsonPropertyName("success")]
        public bool Success { get; set; }

        [JsonPropertyName("data")]
        public Data Data { get; set; } = new();

        [JsonPropertyName("message")]
        public string Message { get; set; } = string.Empty;
    }

    public class Data
    {
        [JsonPropertyName("violations")]
        public List<Violation> Violations { get; set; } = new();
    }

    public class Violation
    {
        [JsonPropertyName("extId")]
        public string ExtId { get; set; } = string.Empty;

        [JsonPropertyName("violationItemView")]
        public ViolationItemView ViolationItemView { get; set; } = new();
    }

    public class ViolationItemView
    {
        [JsonPropertyName("siteAddressInfo")]
        public SiteAddressInfo SiteAddressInfo { get; set; } = new();

        [JsonPropertyName("licensePlate")]
        public string LicensePlate { get; set; } = string.Empty;

        [JsonPropertyName("licensePlateState")]
        public string LicensePlateState { get; set; } = string.Empty;

        [JsonPropertyName("licensePlateImageContext")]
        public string LicensePlateImageContext { get; set; } = string.Empty;

        [JsonPropertyName("licensePlateImagePlate")]
        public string LicensePlateImagePlate { get; set; } = string.Empty;

        [JsonPropertyName("visitStart")]
        public long VisitStart { get; set; }

        [JsonPropertyName("visitEnd")]
        public long VisitEnd { get; set; }

        [JsonPropertyName("violationIssued")]
        public long ViolationIssued { get; set; }

        [JsonPropertyName("parkingAmount")]
        public decimal ParkingAmount { get; set; }

        [JsonPropertyName("fineAmount")]
        public decimal FineAmount { get; set; }

        [JsonPropertyName("feeAmount")]
        public decimal FeeAmount { get; set; }

        [JsonPropertyName("totalAmount")]
        public decimal TotalAmount { get; set; }

        [JsonPropertyName("reason")]
        public Reason Reason { get; set; } = new();
    }

    public class SiteAddressInfo
    {
        [JsonPropertyName("street")]
        public string Street { get; set; } = string.Empty;

        [JsonPropertyName("city")]
        public string City { get; set; } = string.Empty;

        [JsonPropertyName("stateCode")]
        public string StateCode { get; set; } = string.Empty;

        [JsonPropertyName("neighborhood")]
        public string? Neighborhood { get; set; }

        [JsonPropertyName("zip")]
        public string Zip { get; set; } = string.Empty;

        [JsonPropertyName("lat")]
        public double Lat { get; set; }

        [JsonPropertyName("lng")]
        public double Lng { get; set; }

        [JsonPropertyName("entranceAddress")]
        public string? EntranceAddress { get; set; }
    }

    public class Reason
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("description")]
        public string Description { get; set; } = string.Empty;
    }
}
