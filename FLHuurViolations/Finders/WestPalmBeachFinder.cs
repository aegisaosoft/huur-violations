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
using System.Text.RegularExpressions;

namespace FLHuurViolations.Finders
{
    public class WestPalmBeachFinder : AHttpFinder, IHuurAPIFinder
    {
        protected static string _url = "https://wpb.citationportal.com";
        public string Name => "West Palm Beach";
        public string Link => _url;
        
        public event EventHandler<FinderErrorEventArgs>? Error;

        public async Task<List<ParkingViolation>> Find(string licensePlate, string state)
        {
            try
            {
                // Load page and get token
                var initialResponse = await _httpClient.GetAsync(_url);
                var initialHtml = await initialResponse.Content.ReadAsStringAsync();

                var token = ExtractVerificationToken(initialHtml);

                // Prepare form data with correct field names
                var formData = new Dictionary<string, string>
                {
                    { "__RequestVerificationToken", token },
                    { "Type", "PlateStrict" },
                    { "Term", licensePlate },
                    { "AdditionalTerm", state }
                };


                // Submit
                var content = new FormUrlEncodedContent(formData);
                var response = await _httpClient.PostAsync(_url + "/Citation/Search", content);

                // Get HTML
                var html = await response.Content.ReadAsStringAsync();

                // Step 6: Extract info
                var ret = ExtractCitationInfo(html);

                return ret;
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

        private List<ParkingViolation> ExtractCitationInfo(string html)
        {
            List<ParkingViolation> violations = new List<ParkingViolation>();

            if (string.IsNullOrEmpty(html))
                return violations;

            // Check for no results
            if (html.Contains("No citations found", StringComparison.OrdinalIgnoreCase) ||
                html.Contains("No results", StringComparison.OrdinalIgnoreCase))
            {
                return violations;
            }

            // Extract table rows from tbody
            var tbodyPattern = @"<tbody[^>]*class=[""']k-table-tbody[""'][^>]*>(.*?)</tbody>";
            var tbodyMatch = Regex.Match(html, tbodyPattern, RegexOptions.Singleline | RegexOptions.IgnoreCase);

            if (!tbodyMatch.Success)
                return violations;

            var tbody = tbodyMatch.Groups[1].Value;

            // Extract each row
            var rowPattern = @"<tr[^>]*class=[""']k-table-row[^""']*[""'][^>]*>(.*?)</tr>";
            var rowMatches = Regex.Matches(tbody, rowPattern, RegexOptions.Singleline | RegexOptions.IgnoreCase);

            foreach (Match rowMatch in rowMatches)
            {
                var rowHtml = rowMatch.Groups[1].Value;
                var violation = ParseTableRow(rowHtml);

                if (violation != null)
                    violations.Add(violation);
            }

            return violations;
        }

        private ParkingViolation? ParseTableRow(string rowHtml)
        {
            var violation = new ParkingViolation
            {
                Currency = "USD",
                IsActive = true,
                Provider = 1,
                Agency = Name
            };

            // Extract all TD cells
            var tdPattern = @"<td[^>]*>(.*?)</td>";
            var tdMatches = Regex.Matches(rowHtml, tdPattern, RegexOptions.Singleline | RegexOptions.IgnoreCase);

            if (tdMatches.Count < 9)
                return null;

            var cells = new List<string>();
            foreach (Match td in tdMatches)
            {
                cells.Add(CleanHtml(td.Groups[1].Value));
            }

            // CitationNumber
            violation.CitationNumber = CleanText(cells[0]);

            // Link
            var linkMatch = Regex.Match(cells[0], @"href=[""']([^""']+)[""']");
            if (linkMatch.Success)
                violation.Link = _url + linkMatch.Groups[1].Value;

            // Location
            violation.Address = CleanText(cells[1]);

            // State
            violation.State = CleanText(cells[2]);

            // Plate
            violation.Tag = CleanText(cells[3]);

            // Issue Date
            if (DateTime.TryParse(CleanText(cells[5]), out var issueDate))
            {
                violation.IssueDate = issueDate;
                violation.StartDate = issueDate;
            }

            // Due Date
            if (DateTime.TryParse(CleanText(cells[6]), out var dueDate))
                violation.EndDate = dueDate;

            // Status
            var status = CleanText(cells[7]);
            violation.PaymentStatus = Helper.GetStaus(status);
            violation.Note = status;

            violation.Link = _url;
            // Amount
            var amountStr = CleanText(cells[8]).Replace("$", "").Replace(",", "");
            if (decimal.TryParse(amountStr, out var amount))
                violation.Amount = amount;

            violation.FineType = Const.FT_PARKING; // Parking violation

            return violation;
        }

        private string CleanHtml(string html)
        {
            if (string.IsNullOrEmpty(html))
                return string.Empty;

            html = Regex.Replace(html, @"<script[^>]*>.*?</script>", "", RegexOptions.Singleline | RegexOptions.IgnoreCase);
            html = Regex.Replace(html, @"<style[^>]*>.*?</style>", "", RegexOptions.Singleline | RegexOptions.IgnoreCase);
            html = Regex.Replace(html, @"<[^>]+>", " ");
            html = System.Net.WebUtility.HtmlDecode(html);
            html = Regex.Replace(html, @"\s+", " ");

            return html.Trim();
        }

        private string CleanText(string text)
        {
            if (string.IsNullOrEmpty(text))
                return string.Empty;

            text = System.Net.WebUtility.HtmlDecode(text);
            text = Regex.Replace(text, @"\s+", " ");

            return text.Trim();
        }
        private string ExtractVerificationToken(string html)
        {
            var patterns = new[]
            {
            @"name=[""']__RequestVerificationToken[""']\s+(?:type=[""']hidden[""']\s+)?value=[""']([^""']+)[""']",
            @"value=[""']([^""']+)[""']\s+(?:type=[""']hidden[""']\s+)?name=[""']__RequestVerificationToken[""']"
        };

            foreach (var pattern in patterns)
            {
                var match = Regex.Match(html, pattern, RegexOptions.IgnoreCase);
                if (match.Success)
                {
                    return match.Groups[1].Value;
                }
            }

            return string.Empty;
        }
    }
}
