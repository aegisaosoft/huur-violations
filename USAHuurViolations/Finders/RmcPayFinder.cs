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
using HuurVioaltionsAPI.Helpers;
using System.Text.Json;
using USAHuurViolations.Models;

namespace USAHuurViolations.Finders
{
    public class RmcPayFinder : AHttpFinder, IHuurAPIFinder
    {
        protected string _operatorUrl = "https://rmcpay.com/rmcapi/api/violation_index.php/getviolationoperatorinfo?violationnumber=";
        protected string _violationUrl = "https://rmcpay.com/rmcapi/api/violation_index.php/searchviolation?";
        public string Name => "RmcPay";
        public string Link => _violationUrl;
        
        public event EventHandler<FinderErrorEventArgs>? Error;

        public async Task<List<ParkingViolation>> Find(string licensePlate, string state)
        {
            try
            {
                var operatorId = await GetOperatorID(licensePlate, state);

                // Construct the URL with license plate parameter State and Operator Id
                var requestUrl = $"{_violationUrl}operatorid={operatorId}&violationnumber=&stateid={GetStateId(state)}&lpn={licensePlate}&vin=&plate_type_id=&devicenumber=&payment_plan_id=&immobilization_id=&single_violation=0&omsessiondata=&";
                // Make the async HTTP GET request
                var response = await _httpClient.GetAsync(requestUrl);

                if (response.IsSuccessStatusCode)
                {
                    var jsonContent = await response.Content.ReadAsStringAsync();

                    // Parse the JSON response to MetropolisApiResponse object
                    var apiResponse = JsonSerializer.Deserialize<RmcPayResponse>(jsonContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    // Convert to ParkingViolation objects
                    var parkingViolations = new List<ParkingViolation>();
                    if (apiResponse != null && apiResponse.Data != null && apiResponse.Data.Count > 0)
                    {
                        foreach (var violation in apiResponse.Data)
                        {
                            var status = violation.Status;

                            var parkingViolation = new ParkingViolation
                            {
                                CitationNumber = violation.ViolationNumber,
                                NoticeNumber = violation.Number,
                                Agency = violation.OperatorDisplayName,
                                Address = violation.Location ?? violation.Zone,
                                Tag = violation.Lpn,
                                State = violation.VehicleState,
                                IssueDate = ParseDateTime(violation.Date),
                                StartDate = ParseDateTime(violation.Date),
                                EndDate = ParseDateTime(violation.SettlementDate),
                                Amount = decimal.TryParse(violation.AmountInCents, out var amountCents) ? amountCents / 100 : 0,
                                Currency = "USD",
                                PaymentStatus = Helper.GetStaus(status),
                                FineType = Const.FT_PARKING, // Parking violation
                                IsActive = violation.Status.ToLower() != "paid" && violation.Paid != "1",
                                Link = "https://www.rmcpay.com"
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

        public async Task<List<ParkingViolation>> Find(string citation)
        {
            try
            {
                var operatorId = await GetOperatorID(citation);

                // Construct the URL with license plate parameter State and Operator Id
                var requestUrl = $"{_violationUrl}operatorid={operatorId}&violationnumber={citation}&stateid=&lpn=&vin=&plate_type_id=&devicenumber=&payment_plan_id=&immobilization_id=&single_violation=0&omsessiondata=&";
                // Make the async HTTP GET request
                var response = await _httpClient.GetAsync(requestUrl);

                if (response.IsSuccessStatusCode)
                {
                    var jsonContent = await response.Content.ReadAsStringAsync();

                    // Parse the JSON response to MetropolisApiResponse object
                    var apiResponse = JsonSerializer.Deserialize<RmcPayResponse>(jsonContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    // Convert to ParkingViolation objects
                    var parkingViolations = new List<ParkingViolation>();
                    if (apiResponse != null && apiResponse.Data != null && apiResponse.Data.Count > 0)
                    {
                        foreach (var violation in apiResponse.Data)
                        {
                            var status = violation.Status;

                            var parkingViolation = new ParkingViolation
                            {
                                CitationNumber = violation.ViolationNumber,
                                NoticeNumber = violation.Number,
                                Agency = violation.OperatorDisplayName,
                                Address = violation.Location ?? violation.Zone,
                                Tag = violation.Lpn,
                                State = violation.VehicleState,
                                IssueDate = ParseDateTime(violation.Date),
                                StartDate = ParseDateTime(violation.Date),
                                EndDate = ParseDateTime(violation.SettlementDate),
                                Amount = decimal.TryParse(violation.AmountInCents, out var amountCents) ? amountCents / 100 : 0,
                                Currency = "USD",
                                PaymentStatus = Helper.GetStaus(status),
                                FineType = Const.FT_PARKING, // Parking violation
                                IsActive = violation.Status.ToLower() != "paid" && violation.Paid != "1",
                                Link = "https://www.rmcpay.com"
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
                    LicensePlate = citation,
                    State = "",
                    Exception = ex,
                    Message = ex.Message
                });
                return new List<ParkingViolation>();
            }

            return new List<ParkingViolation>();
        }

        public async Task<string> GetOperatorID(string licensePlate, string state)
        {
            try
            {

                // Construct the URL with license plate parameter State and Operator Id

                var requestUrl = $"{_operatorUrl}&stateid={GetStateId(state)}&lpn={licensePlate}&operatorid=0&omsessiondata=&";
                //var requestUrl = $"{_operatorUrl}lookup?method=lpnLookup&lpn={Uri.EscapeDataString(licensePlate)}&lpnState={Uri.EscapeDataString(state)}&includeAll=true/";
                // Make the async HTTP GET request
                var response = await _httpClient.GetAsync(requestUrl);

                if (response.IsSuccessStatusCode)
                {
                    var jsonContent = await response.Content.ReadAsStringAsync();

                    // Parse the JSON response to MetropolisApiResponse object
                    var apiResponse = JsonSerializer.Deserialize<RmcPayOperatorInfoResponse>(jsonContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    // Convert to ParkingViolation objects
                    if (apiResponse != null && apiResponse.Data != null && apiResponse.Data.Operators != null && apiResponse.Data.Operators.Count>0)
                    {
                        foreach (var o in apiResponse.Data.Operators)
                            if (o != null && o.OperatorId != null)
                                return apiResponse.Data.Operators[0].OperatorId;
                    }
                }
            }
            catch (Exception ex)
            {
                // Log exception or handle error appropriately
                Console.WriteLine($"Error calling RmcPay API: {ex.Message}");
                return "";
            }

            return "";
        }
        public async Task<string> GetOperatorID(string citation)
        {
            try
            {

                // Construct the URL with license plate parameter State and Operator Id

                var requestUrl = $"{_operatorUrl}&citation{citation}stateid=&lpn=&operatorid=0&omsessiondata=&";
                //var requestUrl = $"{_operatorUrl}lookup?method=lpnLookup&lpn={Uri.EscapeDataString(licensePlate)}&lpnState={Uri.EscapeDataString(state)}&includeAll=true/";
                // Make the async HTTP GET request
                var response = await _httpClient.GetAsync(requestUrl);

                if (response.IsSuccessStatusCode)
                {
                    var jsonContent = await response.Content.ReadAsStringAsync();

                    // Parse the JSON response to MetropolisApiResponse object
                    var apiResponse = JsonSerializer.Deserialize<RmcPayOperatorInfoResponse>(jsonContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    // Convert to ParkingViolation objects
                    if (apiResponse != null && apiResponse.Data != null && apiResponse.Data.Operators != null && apiResponse.Data.Operators.Count > 0)
                    {
                        foreach (var o in apiResponse.Data.Operators)
                            if (o != null && o.OperatorId != null)
                                return apiResponse.Data.Operators[0].OperatorId;
                    }
                }
            }
            catch (Exception ex)
            {
                // Log exception or handle error appropriately
                Console.WriteLine($"Error calling RmcPay API: {ex.Message}");
                return "";
            }

            return "";
        }

        private string GetStateId(string state)
        {
            // Official RMC Pay state code mapping from their form
            var stateMapping = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                // US States
                {"AL", "81"}, {"Alabama", "81"},
                {"AK", "82"}, {"Alaska", "82"},
                {"AZ", "83"}, {"Arizona", "83"},
                {"AR", "84"}, {"Arkansas", "84"},
                {"CA", "85"}, {"California", "85"},
                {"CO", "86"}, {"Colorado", "86"},
                {"CT", "87"}, {"Connecticut", "87"},
                {"DE", "88"}, {"Delaware", "88"},
                {"DC", "131"}, {"District of Columbia", "131"},
                {"FL", "89"}, {"Florida", "89"},
                {"GA", "90"}, {"Georgia", "90"},
                {"GU", "663"}, {"Guam", "663"},
                {"HI", "91"}, {"Hawaii", "91"},
                {"ID", "92"}, {"Idaho", "92"},
                {"IL", "93"}, {"Illinois", "93"},
                {"IN", "94"}, {"Indiana", "94"},
                {"IA", "95"}, {"Iowa", "95"},
                {"KS", "96"}, {"Kansas", "96"},
                {"KY", "97"}, {"Kentucky", "97"},
                {"LA", "98"}, {"Louisiana", "98"},
                {"ME", "99"}, {"Maine", "99"},
                {"MD", "100"}, {"Maryland", "100"},
                {"MA", "101"}, {"Massachusetts", "101"},
                {"MI", "102"}, {"Michigan", "102"},
                {"MN", "103"}, {"Minnesota", "103"},
                {"MS", "104"}, {"Mississippi", "104"},
                {"MO", "105"}, {"Missouri", "105"},
                {"MT", "106"}, {"Montana", "106"},
                {"NE", "107"}, {"Nebraska", "107"},
                {"NV", "108"}, {"Nevada", "108"},
                {"NH", "109"}, {"New Hampshire", "109"},
                {"NJ", "110"}, {"New Jersey", "110"},
                {"NM", "111"}, {"New Mexico", "111"},
                {"NY", "112"}, {"New York", "112"},
                {"NC", "113"}, {"North Carolina", "113"},
                {"ND", "114"}, {"North Dakota", "114"},
                {"OH", "115"}, {"Ohio", "115"},
                {"OK", "116"}, {"Oklahoma", "116"},
                {"OR", "117"}, {"Oregon", "117"},
                {"PA", "118"}, {"Pennsylvania", "118"},
                {"PR", "495"}, {"Puerto Rico", "495"},
                {"RI", "119"}, {"Rhode Island", "119"},
                {"SC", "120"}, {"South Carolina", "120"},
                {"SD", "121"}, {"South Dakota", "121"},
                {"TN", "122"}, {"Tennessee", "122"},
                {"TX", "123"}, {"Texas", "123"},
                {"UT", "124"}, {"Utah", "124"},
                {"VT", "125"}, {"Vermont", "125"},
                {"VI", "613"}, {"Virgin Islands", "613"},
                {"VA", "126"}, {"Virginia", "126"},
                {"WA", "127"}, {"Washington", "127"},
                {"WV", "128"}, {"West Virginia", "128"},
                {"WI", "129"}, {"Wisconsin", "129"},
                {"WY", "130"}, {"Wyoming", "130"},
                
                // Canadian Provinces
                {"AB", "193"}, {"Alberta", "193"},
                {"BC", "194"}, {"British Columbia", "194"},
                {"MB", "195"}, {"Manitoba", "195"},
                {"NB", "196"}, {"New Brunswick", "196"},
                {"NL", "197"}, {"Newfoundland and Labrador", "197"},
                {"NT", "198"}, {"Northwest Territories", "198"},
                {"NS", "199"}, {"Nova Scotia", "199"},
                {"NU", "200"}, {"Nunavut", "200"},
                {"ON", "201"}, {"Ontario", "201"},
                {"PE", "202"}, {"Prince Edward Island", "202"},
                {"QC", "203"}, {"Quebec", "203"},
                {"SK", "204"}, {"Saskatchewan", "204"},
                {"YT", "205"}, {"Yukon", "205"}
            };

            var stateId = stateMapping.ContainsKey(state) ? stateMapping[state] : state;

            return stateId;
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
