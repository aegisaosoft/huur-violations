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
using HuurApi.Models;
using HuurVioaltionsAPI;
using HuurVioaltionsAPI.Abstr;
using System.Text.Json;
using USAHuurViolations.Models;

namespace USAHuurViolations.Finders
{
    public class VanGuardFinder : AHttpFinder, IHuurAPIFinder
    {
        protected string _url = "https://www.payparkingnotice.com/api/";

        public string Name => "VanGuard";
        public string Link => _url;
        
        public event EventHandler<FinderErrorEventArgs>? Error;

        public async Task<List<ParkingViolation>> Find(string licensePlate, string state)
        {
            try
            {

                // Construct the URL with license plate parameter
                var requestUrl = $"{_url}lookup?method=lpnLookup&lpn={Uri.EscapeDataString(licensePlate)}&lpnState={Uri.EscapeDataString(state)}&includeAll=true/";
                // Make the async HTTP GET request
                var response = await _httpClient.GetAsync(requestUrl);

                if (response.IsSuccessStatusCode)
                {
                    var jsonContent = await response.Content.ReadAsStringAsync();

                    // Parse the JSON response to MetropolisApiResponse object
                    var apiResponse = JsonSerializer.Deserialize<VanGuardResponse>(jsonContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    // Convert to ParkingViolation objects
                    var parkingViolations = new List<ParkingViolation>();
                    if (apiResponse != null && apiResponse.RecordsFound > 0 && apiResponse.Notices != null)
                    {
                        foreach (var violation in apiResponse.Notices)
                        {
                            var parkingViolation = new ParkingViolation
                            {
                                NoticeNumber = violation.NoticeNumber,
                                Agency = Name,
                                Address = violation.LotAddress,
                                Tag = violation.Lpn,
                                State = violation.LpnState,
                                IssueDate = violation.NoticeDate == null ? null : DateTimeOffset.FromUnixTimeMilliseconds(violation.NoticeDate.Ts).DateTime,
                                StartDate = ParseDateTime(violation.EntryTime),
                                EndDate = ParseDateTime(violation.ExitTime),
                                Amount = decimal.TryParse(violation.AmountDue, out var amount) ? amount : 0,
                                Currency = "USD",
                                PaymentStatus = violation.TicketStatus.ToLower() != "paid" ? Const.P_NEW : Const.P_PAID,
                                FineType = Const.FT_PARKING, // Parking violation
                                IsActive = violation.TicketStatus.ToLower() != "paid",
                                Link = _url
                            };
                            parkingViolations.Add(parkingViolation);
                        }
                    }

                    return parkingViolations;
                }
            }
            catch (Exception ex)
            {
                Error?.Invoke(this, new FinderErrorEventArgs
                {
                    FinderName = Name,
                    LicensePlate = licensePlate,
                    State = state,
                    Exception = ex,
                    Message = ex.Message
                });
                return new List<ParkingViolation>();
            }

            return new List<ParkingViolation>();
        }

        private DateTime ParseDateTime(string dateTimeString)
        {
            if (DateTime.TryParse(dateTimeString, out var result))
            {
                return result;
            }
            return DateTime.MinValue;
        }
    }
}
