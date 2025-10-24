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
    public class VanGuardResponse
    {
        [JsonPropertyName("recordsFound")]
        public int RecordsFound { get; set; }

        [JsonPropertyName("notices")]
        public List<Notice> Notices { get; set; } = new();
    }

    public class Notice
    {
        [JsonPropertyName("sessionId")]
        public string? SessionId { get; set; }

        [JsonPropertyName("notice")]
        public string NoticeNumber { get; set; } = string.Empty;

        [JsonPropertyName("noticeDate")]
        public NoticeDate NoticeDate { get; set; } = new();

        [JsonPropertyName("entryTime")]
        public string EntryTime { get; set; } = string.Empty;

        [JsonPropertyName("exitTime")]
        public string ExitTime { get; set; } = string.Empty;

        [JsonPropertyName("ticketStatus")]
        public string TicketStatus { get; set; } = string.Empty;

        [JsonPropertyName("ticketTimeStatus")]
        public string TicketTimeStatus { get; set; } = string.Empty;

        [JsonPropertyName("isHandheld")]
        public bool IsHandheld { get; set; }

        [JsonPropertyName("inCollections")]
        public bool InCollections { get; set; }

        [JsonPropertyName("lpn")]
        public string Lpn { get; set; } = string.Empty;

        [JsonPropertyName("lpnId")]
        public string LpnId { get; set; } = string.Empty;

        [JsonPropertyName("lpnState")]
        public string LpnState { get; set; } = string.Empty;

        [JsonPropertyName("lotId")]
        public string LotId { get; set; } = string.Empty;

        [JsonPropertyName("lotAddress")]
        public string LotAddress { get; set; } = string.Empty;

        [JsonPropertyName("amountDue")]
        public string AmountDue { get; set; } = string.Empty;

        [JsonPropertyName("ticketPaidOn")]
        public string? TicketPaidOn { get; set; }

        [JsonPropertyName("breakdown")]
        public Breakdown Breakdown { get; set; } = new();

        [JsonPropertyName("images")]
        public Images Images { get; set; } = new();

        [JsonPropertyName("surchargeName")]
        public string SurchargeName { get; set; } = string.Empty;
    }

    public class NoticeDate
    {
        [JsonPropertyName("ts")]
        public long Ts { get; set; }

        [JsonPropertyName("_zone")]
        public Zone? Zone { get; set; }

        [JsonPropertyName("loc")]
        public Loc? Loc { get; set; }

        [JsonPropertyName("invalid")]
        public string? Invalid { get; set; }

        [JsonPropertyName("weekData")]
        public string? WeekData { get; set; }

        [JsonPropertyName("localWeekData")]
        public string? LocalWeekData { get; set; }

        [JsonPropertyName("c")]
        public C C { get; set; } = new();

        [JsonPropertyName("o")]
        public int O { get; set; }

        [JsonPropertyName("isLuxonDateTime")]
        public bool IsLuxonDateTime { get; set; }
    }

    public class Zone
    {
        // Empty object in the JSON structure
    }

    public class Loc
    {
        [JsonPropertyName("locale")]
        public string Locale { get; set; } = string.Empty;

        [JsonPropertyName("numberingSystem")]
        public string? NumberingSystem { get; set; }

        [JsonPropertyName("outputCalendar")]
        public string? OutputCalendar { get; set; }

        [JsonPropertyName("weekSettings")]
        public string? WeekSettings { get; set; }

        [JsonPropertyName("intl")]
        public string Intl { get; set; } = string.Empty;

        [JsonPropertyName("weekdaysCache")]
        public WeekdaysCache? WeekdaysCache { get; set; }

        [JsonPropertyName("monthsCache")]
        public MonthsCache? MonthsCache { get; set; }

        [JsonPropertyName("meridiemCache")]
        public string? MeridiemCache { get; set; }

        [JsonPropertyName("eraCache")]
        public EraCache? EraCache { get; set; }

        [JsonPropertyName("specifiedLocale")]
        public string? SpecifiedLocale { get; set; }

        [JsonPropertyName("fastNumbersCached")]
        public string? FastNumbersCached { get; set; }
    }

    public class WeekdaysCache
    {
        [JsonPropertyName("format")]
        public Dictionary<string, string>? Format { get; set; }

        [JsonPropertyName("standalone")]
        public Dictionary<string, string>? Standalone { get; set; }
    }

    public class MonthsCache
    {
        [JsonPropertyName("format")]
        public Dictionary<string, string>? Format { get; set; }

        [JsonPropertyName("standalone")]
        public Dictionary<string, string>? Standalone { get; set; }
    }

    public class EraCache
    {
        // Empty object in the JSON structure
    }

    public class C
    {
        [JsonPropertyName("year")]
        public int Year { get; set; }

        [JsonPropertyName("month")]
        public int Month { get; set; }

        [JsonPropertyName("day")]
        public int Day { get; set; }

        [JsonPropertyName("hour")]
        public int Hour { get; set; }

        [JsonPropertyName("minute")]
        public int Minute { get; set; }

        [JsonPropertyName("second")]
        public int Second { get; set; }

        [JsonPropertyName("millisecond")]
        public int Millisecond { get; set; }
    }

    public class Breakdown
    {
        [JsonPropertyName("locationTicketAmount")]
        public string LocationTicketAmount { get; set; } = string.Empty;

        [JsonPropertyName("locationTicketDiscount")]
        public string LocationTicketDiscount { get; set; } = string.Empty;

        [JsonPropertyName("tax")]
        public string Tax { get; set; } = string.Empty;

        [JsonPropertyName("surchargeAmount")]
        public string SurchargeAmount { get; set; } = string.Empty;

        [JsonPropertyName("totalTicketPriceWithTax")]
        public string TotalTicketPriceWithTax { get; set; } = string.Empty;

        [JsonPropertyName("surchargeName")]
        public string SurchargeName { get; set; } = string.Empty;
    }

    public class Images
    {
        [JsonPropertyName("vehicle_entry")]
        public string VehicleEntry { get; set; } = string.Empty;

        [JsonPropertyName("vehicle_exit")]
        public string VehicleExit { get; set; } = string.Empty;
    }
}
