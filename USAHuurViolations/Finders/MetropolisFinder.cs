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
    public class MetropolisFinder : AHttpFinder, IHuurAPIFinder
    {
        protected string _url = "https://site.metropolis.io/api/violation/customer/violations/";

        public string Name => "Metropolis";
        public string Link => _url;
        
        public event EventHandler<FinderErrorEventArgs>? Error;

        public async Task<List<ParkingViolation>> Find(string licensePlate, string state)
        {
            try
            {

                // Construct the URL with license plate parameter
                var requestUrl = $"{_url}search?licensePlateText={Uri.EscapeDataString(licensePlate)}&licensePlateState={Uri.EscapeDataString(state)}";
                // Make the async HTTP GET request
                var response = await _httpClient.GetAsync(requestUrl);

                if (response.IsSuccessStatusCode)
                {
                    var jsonContent = await response.Content.ReadAsStringAsync();

                    // Parse the JSON response to MetropolisApiResponse object
                    var apiResponse = JsonSerializer.Deserialize<MetropolisApiResponse>(jsonContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    // Convert to ParkingViolation objects
                    var parkingViolations = new List<ParkingViolation>();
                    if (apiResponse != null && apiResponse.Success && apiResponse.Data?.Violations != null)
                    {
                        foreach (var violation in apiResponse.Data.Violations)
                        {
                            var parkingViolation = new ParkingViolation
                            {
                                //CitationNumber = violation.ExtId,
                                NoticeNumber = violation.ExtId,
                                Agency = Name,
                                Address = $"{violation.ViolationItemView.SiteAddressInfo.Street}, {violation.ViolationItemView.SiteAddressInfo.City}, {violation.ViolationItemView.SiteAddressInfo.StateCode} {violation.ViolationItemView.SiteAddressInfo.Zip}",
                                Tag = violation.ViolationItemView.LicensePlate,
                                State = violation.ViolationItemView.LicensePlateState,
                                IssueDate = DateTimeOffset.FromUnixTimeMilliseconds(violation.ViolationItemView.ViolationIssued).DateTime,
                                StartDate = DateTimeOffset.FromUnixTimeMilliseconds(violation.ViolationItemView.VisitStart).DateTime,
                                EndDate = DateTimeOffset.FromUnixTimeMilliseconds(violation.ViolationItemView.VisitEnd).DateTime,
                                Amount = violation.ViolationItemView.TotalAmount,
                                Currency = "USD",
                                PaymentStatus = Const.P_NEW,
                                FineType = Const.FT_PARKING, // Parking violation
                                IsActive = true,
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
    }
}
