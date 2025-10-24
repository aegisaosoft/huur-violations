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
using System.Text.RegularExpressions;

namespace FLHuurViolations.Finders
{
    public class BlinkayFinder : AHttpFinder, IHuurAPIFinder
    {
        protected static string _url = "https://webapp-usa.blinkay.app";
        public string Name => "Blinkay";
        public string Link => _url;

        private readonly string _installationId = "110010";
        
        public event EventHandler<FinderErrorEventArgs>? Error;

        public async Task<List<ParkingViolation>> Find(string licensePlate, string state)
        {
            try
            {
                // Step 1: Get the form page to extract CSRF token
                var formUrl = $"{_url}/integraMobile/Fine/MultiFine?InstallationId={_installationId}&Culture=en-US";
                var formResponse = await _httpClient.GetAsync(formUrl);
                var formHtml = await formResponse.Content.ReadAsStringAsync();

                // Extract CSRF token
                var token = ExtractToken(formHtml);
                if (string.IsNullOrEmpty(token))
                {
                    throw new Exception("Failed to extract CSRF token");
                }

                // Extract other hidden fields
                var installationList = ExtractHiddenField(formHtml, "InstallationList");
                var standardInstallationList = ExtractHiddenField(formHtml, "StandardInstallationList");
                var forceInstallationId = ExtractHiddenField(formHtml, "ForceInstallationId");

                // Step 2: Submit the form with plate number
                var postUrl = $"{_url}/integraMobile/Fine/MultiDetails";
                var formData = new Dictionary<string, string>
                {
                    { "__RequestVerificationToken", token },
                    { "Plate", licensePlate.Trim().ToUpper() },
                    { "TicketNumber", "" },
                    { "ForceInstallationId", forceInstallationId ?? _installationId },
                    { "InstallationList", installationList ?? _installationId },
                    { "StandardInstallationList", standardInstallationList ?? "" }
                };

                var content = new FormUrlEncodedContent(formData);
                var response = await _httpClient.PostAsync(postUrl, content);
                var responseHtml = await response.Content.ReadAsStringAsync();

                // Step 3: Parse the response for violation details
                var violations = ParseViolations(responseHtml, licensePlate, state);

                return violations;
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
            }

            return new List<ParkingViolation>();
        }

        private string? ExtractToken(string html)
        {
            var match = Regex.Match(html, @"name=""__RequestVerificationToken""[^>]*value=""([^""]+)""");
            return match.Success ? match.Groups[1].Value : null;
        }


        private string? ExtractHiddenField(string html, string fieldName)
        {
            var pattern = $@"name=""{fieldName}""[^>]*value=""([^""]*)""";
            var match = Regex.Match(html, pattern);
            return match.Success ? match.Groups[1].Value : null;
        }

        private List<ParkingViolation> ParseViolations(string html, string plateNumber, string state)
        {
            List<ParkingViolation> violations = new();
            // Check if there's an error message (no violations found)
            if (string.IsNullOrEmpty(html) ||
                html.Contains("No records found") ||
                html.Contains("no violations") ||
                html.Contains("not found") ||
                !html.Contains("multiticket_row"))
            {
                return violations;
            }

            // Find all multiticket_row divs (excluding the header)
            var rowPattern = @"<div class=""multiticket_row\s*"">\s*<div class=""multiticket_col1"">.*?<div class=""clear""></div>\s*</div>";
            var matches = Regex.Matches(html, rowPattern, RegexOptions.Singleline);

            foreach (Match match in matches)
            {
                var row = match.Value;
                var violation = new ParkingViolation 
                { 
                    State = state,
                    Agency=Name,
                    Link = _url
                };

                // Extract ticket number from checkbox value
                var ticketMatch = Regex.Match(row, @"name=""CheckedTickets""\s+value=""(\d+)""");
                if (ticketMatch.Success)
                {
                    violation.NoticeNumber = ticketMatch.Groups[1].Value;
                }

                // Extract amount from data-amount attribute (in cents)
                var amountMatch = Regex.Match(row, @"data-amount=""(\d+)""");
                if (amountMatch.Success && int.TryParse(amountMatch.Groups[1].Value, out var amountCents))
                {
                    violation.Amount = amountCents / 100m;
                }

                // Extract plate number from multiticket_col3
                var plateMatch = Regex.Match(row, @"<div class=""multiticket_col3"">([^<]+)</div>");
                if (plateMatch.Success)
                {
                    violation.Tag = plateMatch.Groups[1].Value.Trim();
                }

                // Extract date from multiticket_col4
                var dateMatch = Regex.Match(row, @"<div class=""multiticket_col4"">([^<]+)</div>");
                if (dateMatch.Success)
                {
                    DateTime.TryParse(dateMatch.Groups[1].Value.Trim(),out var date);
                    violation.IssueDate = date;
                }

                // Determine status (if checkbox is checked, it's unpaid and selected for payment)
                violation.PaymentStatus = row.Contains("checked") ? Const.P_PAID : Const.P_NEW;

                // Only add if we have at least a ticket number
                    violations.Add(violation);
                }

            return violations;
        }
    }
}