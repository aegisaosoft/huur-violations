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

namespace FLHuurViolations.Models
{
    public class ParkingComplienceApiResponse
    {
        [JsonPropertyName("_id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("lot")]
        public Lot Lot { get; set; } = new();

        [JsonPropertyName("camera1")]
        public string Camera1 { get; set; } = string.Empty;

        [JsonPropertyName("plateNumber")]
        public string PlateNumber { get; set; } = string.Empty;

        [JsonPropertyName("country")]
        public string Country { get; set; } = string.Empty;

        [JsonPropertyName("plate1")]
        public string Plate1 { get; set; } = string.Empty;

        [JsonPropertyName("vehicle1")]
        public string Vehicle1 { get; set; } = string.Empty;

        [JsonPropertyName("entryTime")]
        public DateTime EntryTime { get; set; }

        [JsonPropertyName("status")]
        public string Status { get; set; } = string.Empty;

        [JsonPropertyName("noticeNumber")]
        public string NoticeNumber { get; set; } = string.Empty;

        [JsonPropertyName("fine")]
        public decimal Fine { get; set; }

        [JsonPropertyName("printedAt")]
        public DateTime? PrintedAt { get; set; }

        [JsonPropertyName("printedAts")]
        public List<DateTime> PrintedAts { get; set; } = new();

        [JsonPropertyName("createdAt")]
        public DateTime CreatedAt { get; set; }

        [JsonPropertyName("updatedAt")]
        public DateTime UpdatedAt { get; set; }

        [JsonPropertyName("__v")]
        public int Version { get; set; }

        [JsonPropertyName("camera2")]
        public string Camera2 { get; set; } = string.Empty;

        [JsonPropertyName("exitTime")]
        public DateTime? ExitTime { get; set; }

        [JsonPropertyName("plate2")]
        public string Plate2 { get; set; } = string.Empty;

        [JsonPropertyName("vehicle2")]
        public string Vehicle2 { get; set; } = string.Empty;
    }

    public class Lot
    {
        [JsonPropertyName("_id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("enterToken")]
        public string EnterToken { get; set; } = string.Empty;

        [JsonPropertyName("exitToken")]
        public string ExitToken { get; set; } = string.Empty;

        [JsonPropertyName("url")]
        public string Url { get; set; } = string.Empty;

        [JsonPropertyName("flowbirdZone")]
        public string? FlowbirdZone { get; set; }

        [JsonPropertyName("siteCode")]
        public string SiteCode { get; set; } = string.Empty;

        [JsonPropertyName("percentage")]
        public int Percentage { get; set; }

        [JsonPropertyName("hourlyRate")]
        public decimal HourlyRate { get; set; }

        [JsonPropertyName("payTime")]
        public int PayTime { get; set; }

        [JsonPropertyName("address")]
        public string Address { get; set; } = string.Empty;

        [JsonPropertyName("cover")]
        public string Cover { get; set; } = string.Empty;

        [JsonPropertyName("pApps")]
        public List<string> PApps { get; set; } = new();

        [JsonPropertyName("owners")]
        public List<string> Owners { get; set; } = new();

        [JsonPropertyName("cameras")]
        public List<string> Cameras { get; set; } = new();

        [JsonPropertyName("dates")]
        public List<string> Dates { get; set; } = new();

        [JsonPropertyName("createdAt")]
        public DateTime CreatedAt { get; set; }

        [JsonPropertyName("updatedAt")]
        public DateTime UpdatedAt { get; set; }

        [JsonPropertyName("__v")]
        public int Version { get; set; }

        [JsonPropertyName("payingFee")]
        public decimal PayingFee { get; set; }

        [JsonPropertyName("violationFee")]
        public decimal ViolationFee { get; set; }

        [JsonPropertyName("ticketThreshold")]
        public int TicketThreshold { get; set; }

        [JsonPropertyName("towEmail")]
        public string TowEmail { get; set; } = string.Empty;

        [JsonPropertyName("firstFine")]
        public decimal FirstFine { get; set; }

        [JsonPropertyName("secondFine")]
        public decimal SecondFine { get; set; }

        [JsonPropertyName("thirdFine")]
        public decimal ThirdFine { get; set; }

        [JsonPropertyName("validations")]
        public List<Validation> Validations { get; set; } = new();

        [JsonPropertyName("priceId")]
        public string PriceId { get; set; } = string.Empty;

        [JsonPropertyName("customPrices")]
        public List<CustomPrice> CustomPrices { get; set; } = new();

        [JsonPropertyName("stripeAccountID")]
        public string StripeAccountID { get; set; } = string.Empty;

        [JsonPropertyName("stripePublicKey")]
        public string StripePublicKey { get; set; } = string.Empty;

        [JsonPropertyName("stripeSecretKey")]
        public string StripeSecretKey { get; set; } = string.Empty;

        [JsonPropertyName("stripeWebhookKey")]
        public string StripeWebhookKey { get; set; } = string.Empty;

        [JsonPropertyName("T2Locations")]
        public List<string> T2Locations { get; set; } = new();

        [JsonPropertyName("leeway")]
        public int Leeway { get; set; }

        [JsonPropertyName("employees")]
        public List<string> Employees { get; set; } = new();

        [JsonPropertyName("live")]
        public bool Live { get; set; }

        [JsonPropertyName("transactionFee")]
        public decimal TransactionFee { get; set; }

        [JsonPropertyName("paybyphone")]
        public string Paybyphone { get; set; } = string.Empty;

        [JsonPropertyName("logo")]
        public string Logo { get; set; } = string.Empty;

        [JsonPropertyName("dockLocation")]
        public string? DockLocation { get; set; }
    }

    public class Validation
    {
        [JsonPropertyName("user")]
        public string User { get; set; } = string.Empty;

        [JsonPropertyName("duration")]
        public int Duration { get; set; }

        [JsonPropertyName("price")]
        public decimal Price { get; set; }

        [JsonPropertyName("_id")]
        public string Id { get; set; } = string.Empty;
    }

    public class CustomPrice
    {
        [JsonPropertyName("activeDays")]
        public List<int> ActiveDays { get; set; } = new();

        [JsonPropertyName("rangeStart")]
        public string RangeStart { get; set; } = string.Empty;

        [JsonPropertyName("rangeEnd")]
        public string RangeEnd { get; set; } = string.Empty;

        [JsonPropertyName("specificDate")]
        public string? SpecificDate { get; set; }

        [JsonPropertyName("expirationDate")]
        public DateTime ExpirationDate { get; set; }

        [JsonPropertyName("duration")]
        public decimal Duration { get; set; }

        [JsonPropertyName("price")]
        public decimal Price { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; } = string.Empty;

        [JsonPropertyName("detail")]
        public string Detail { get; set; } = string.Empty;

        [JsonPropertyName("_id")]
        public string Id { get; set; } = string.Empty;
    }
}