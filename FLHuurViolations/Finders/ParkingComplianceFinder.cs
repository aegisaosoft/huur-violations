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

using HuurVioaltionsAPI;
using System.Text.Json;
using FLHuurViolations.Models;
using HuurApi.Models;
using HuurVioaltionsAPI.Abstr;

namespace FLHuurViolations.Finders
{
    public class ParkingComplianceFinder : AHttpFinder, IHuurAPIFinder
    {
        protected string _url = "https://api.cpmdashboard.com/v1/violationapp/violations/";
        public string Name => "Parking Compliance";
        public string Link => _url;
        
        public event EventHandler<FinderErrorEventArgs>? Error;

        public async Task<List<ParkingViolation>> Find(string licensePlate, string state)
        {
            try
            {
                // Construct the URL with license plate parameter
                var requestUrl = $"{_url}{Uri.EscapeDataString(licensePlate)}";

                // Make the async HTTP GET request
                var response = await _httpClient.GetAsync(requestUrl);
                
                if (response.IsSuccessStatusCode)
                {
                    var jsonContent = await response.Content.ReadAsStringAsync();
                    
                    // Parse the JSON response to ViolationApiResponse objects
                    var apiResponses = JsonSerializer.Deserialize<List<ParkingComplienceApiResponse>>(jsonContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                    
                    // Convert to ParkingViolation objects
                    var parkingViolations = new List<ParkingViolation>();
                    if (apiResponses != null)
                    {
                        for (int i = 0; i < apiResponses.Count; i++)
                        {
                            var apiResponse = apiResponses[i];
                            
                            var parkingViolation = new ParkingViolation
                            {
                                //CitationNumber = apiResponse.NoticeNumber,
                                NoticeNumber = apiResponse.NoticeNumber,
                                Agency = Name,
                                Address = apiResponse.Lot.Address,
                                StartDate = apiResponse.EntryTime,
                                EndDate = apiResponse.ExitTime,
                                Tag = apiResponse.PlateNumber,
                                State = state,
                                IssueDate = apiResponse.EntryTime,
                                Amount = apiResponse.Fine,
                                Currency = "USD",
                                PaymentStatus = apiResponse.Status == "PAID" ? Const.P_PAID : Const.P_NEW,
                                FineType = Const.FT_PARKING, // Parking violation
                                IsActive = apiResponse.Status != "RESOLVED",
                                Link = _url
                            };
                            
                            parkingViolations.Add(parkingViolation);
                        }
                    }
                    
                    return parkingViolations;
                }
                else
                {
                    // Log error or handle non-success status codes
                    Console.WriteLine($"API call failed with status: {response.StatusCode}");
                    return new List<ParkingViolation>();
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
        }
    }
}
