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

using HtmlAgilityPack;
using HuurApi.Models;
using System.Net;

namespace HuurVioaltionsAPI.Helpers
{
    public class PortalHelper
    {
        private static readonly Dictionary<string, string> StateToIdMap = new Dictionary<string, string>
    {
        { "AL", "1" },  { "AK", "2" },  { "AZ", "3" },  { "AR", "4" },  { "CA", "5" },
        { "CO", "6" },  { "CT", "7" },  { "DE", "8" },  { "DC", "9" },  { "FL", "10" },
        { "GA", "11" }, { "HI", "12" }, { "ID", "13" }, { "IL", "14" }, { "IN", "15" },
        { "IA", "16" }, { "KS", "17" }, { "KY", "18" }, { "LA", "19" }, { "ME", "20" },
        { "MD", "21" }, { "MA", "22" }, { "MI", "23" }, { "MN", "24" }, { "MS", "25" },
        { "MO", "26" }, { "MT", "27" }, { "NE", "28" }, { "NV", "29" }, { "NH", "30" },
        { "NJ", "31" }, { "NM", "32" }, { "NY", "33" }, { "NC", "34" }, { "ND", "35" },
        { "OH", "36" }, { "OK", "37" }, { "OR", "38" }, { "PA", "39" }, { "RI", "40" },
        { "SC", "41" }, { "SD", "42" }, { "TN", "43" }, { "TX", "44" }, { "UT", "45" },
        { "VT", "46" }, { "VA", "47" }, { "WA", "48" }, { "WV", "49" }, { "WI", "50" },
        { "WY", "51" }
    };

        private readonly HttpClient _client;
        private readonly string BaseUrl = string.Empty; //"https://fortlauderdaleparking.t2hosted.com";
        private readonly string PortalUrl = string.Empty;
        private readonly string SearchUrl = string.Empty;
        private readonly string Agensy = string.Empty;
        public PortalHelper(string baseUrl, string agensy)
        {
            BaseUrl = baseUrl;
            PortalUrl = BaseUrl + "/Account/Portal";
            SearchUrl = BaseUrl + "/Account/Citations/Search";

            Agensy = agensy;

            var cookieJar = new CookieContainer();
            var handler = new HttpClientHandler
            {
                UseCookies = true,
                CookieContainer = cookieJar,
                AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip
            };

            _client = new HttpClient(handler);

            _client.DefaultRequestHeaders.UserAgent.ParseAdd(
                "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/127.0.0.0 Safari/537.36"
            );
            _client.DefaultRequestHeaders.Accept.ParseAdd("application/json, text/plain, */*");
            _client.DefaultRequestHeaders.AcceptLanguage.ParseAdd("en-US,en;q=0.9");
        }

        /// <summary>
        /// Extract links from the portal page
        /// </summary>
        public async Task<List<LinkInfo>> ExtractLinksFromPortal()
        {
            try
            {
                var response = await _client.GetAsync(PortalUrl);
                var html = await response.Content.ReadAsStringAsync();

                var doc = new HtmlDocument();
                doc.LoadHtml(html);

                var links = new List<LinkInfo>();

                // Extract all links
                var linkNodes = doc.DocumentNode.SelectNodes("//a[@href]");
                if (linkNodes != null)
                {
                    foreach (var link in linkNodes)
                    {
                        var href = link.GetAttributeValue("href", "");
                        var text = link.InnerText.Trim();

                        if (!string.IsNullOrEmpty(href))
                        {
                            // Make absolute URL if relative
                            var absoluteUrl = href.StartsWith("http") ? href : BaseUrl + href;
                            links.Add(new LinkInfo { Url = absoluteUrl, Text = text });
                        }
                    }
                }

                return links;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error extracting links: {ex.Message}");
                return new List<LinkInfo>();
            }
        }

        /// <summary>
        /// Convert state abbreviation to state ID number
        /// </summary>
        public static string GetStateId(string stateCode)
        {
            stateCode = stateCode?.ToUpper() ?? string.Empty;
            if (StateToIdMap.TryGetValue(stateCode, out string? stateId))
            {
                return stateId;
            }
            throw new ArgumentException($"Invalid state code: {stateCode}");
        }

        /// <summary>
        /// Search for citation by license plate and state
        /// </summary>
        public async Task<List<ParkingViolation>> SearchCitation(string plateNumber, string stateCode)
        {
            try
            {
                // Step 1: Load portal page to get cookies and token
                Console.WriteLine("Loading portal page...");
                var getResponse = await _client.GetAsync(PortalUrl);
                var html = await getResponse.Content.ReadAsStringAsync();

                // Step 2: Parse verification token
                var doc = new HtmlDocument();
                doc.LoadHtml(html);

                var tokenNode = doc.DocumentNode.SelectSingleNode("//input[@name='__RequestVerificationToken']");
                string token = tokenNode?.GetAttributeValue("value", "") ?? string.Empty;

                if (string.IsNullOrEmpty(token))
                {
                }

                Console.WriteLine("Verification token obtained.");

                // Step 3: Convert state code to ID
                string stateId = GetStateId(stateCode);
                Console.WriteLine($"State {stateCode} -> ID {stateId}");

                // Step 4: Prepare POST request
                var formData = new Dictionary<string, string>
            {
                { "__RequestVerificationToken", token ?? string.Empty },
                { "PlateNumber", plateNumber.ToUpper() },
                { "StateId", stateId },
                { "CitationNumber", "" },
            };

                var content = new FormUrlEncodedContent(formData);

                // Step 5: Set headers
                _client.DefaultRequestHeaders.Remove("X-Requested-With");
                _client.DefaultRequestHeaders.Remove("Referer");
                _client.DefaultRequestHeaders.Add("X-Requested-With", "XMLHttpRequest");
                _client.DefaultRequestHeaders.Add("Referer", PortalUrl);
                _client.DefaultRequestHeaders.Add("Origin", BaseUrl);

                // Step 6: Submit search
                Console.WriteLine($"Searching for plate {plateNumber} in state {stateCode}...");
                var postResponse = await _client.PostAsync(SearchUrl, content);
                string result = await postResponse.Content.ReadAsStringAsync();

                string? resultPath = GetResultPath(result);

                if (resultPath == null)
                    return new List<ParkingViolation>();

                string resultUrl = BaseUrl + resultPath;

                var response = await _client.GetAsync(resultUrl);

                var resultHtml = await response.Content.ReadAsStringAsync();

                return ParseCitations(resultHtml, plateNumber, stateCode);
            }
            catch (Exception)
            {
                return new List<ParkingViolation>();
            }
        }

        /// <summary>
        /// GetResultPath check if result exixsts
        /// </summary>
        /// <param name="result"></param>
        /// <returns></returns>
        public string? GetResultPath(string result)
        {
            if (result.StartsWith("document.location"))
                return result.Substring(result.IndexOf("/")).Replace(";", "").Replace("'", "");

            return null;
        }

        /// <summary>
        /// Parse citation results from HTML response
        /// </summary>
        public List<ParkingViolation> ParseCitations(string html, string plateNumber, string stateCode)
        {
            var citations = new List<ParkingViolation>();
            try
            {
                var doc = new HtmlDocument();
                doc.LoadHtml(html);

                // Select all citation rows (skip the header row)
                var citationRows = doc.DocumentNode.SelectNodes("//table[@id='citations-list-table']//tr[starts-with(@id, 'citation')]");

                if (citationRows != null)
                {
                    foreach (var row in citationRows)
                    {
                        try
                        {
                            var cells = row.SelectNodes(".//td");
                            if (cells != null && cells.Count >= 6)
                            {
                                var citation = new ParkingViolation();

                                // Extract ID from the row id attribute (e.g., "citation2793790" -> 2793790)
                                var rowId = row.GetAttributeValue("id", "");
                                if (!string.IsNullOrEmpty(rowId))
                                {
                                    citation.Note = rowId;
                                }

                                // Citation Number (cell 0)
                                citation.CitationNumber = cells[0].InnerText?.Trim();

                                // Status (cell 1) - map to PaymentStatus
                                var status = cells[1].InnerText?.Trim();
                                citation.PaymentStatus = Helper.GetStaus(status);

                                // Amount (cell 2) - parse dollar amount
                                var balanceText = cells[2].InnerText?.Trim();
                                if (!string.IsNullOrEmpty(balanceText))
                                {
                                    // Remove "$" and any other non-numeric characters except decimal point
                                    balanceText = System.Text.RegularExpressions.Regex.Replace(balanceText, @"[^\d.]", "");
                                    if (decimal.TryParse(balanceText, out decimal amount))
                                    {
                                        citation.Amount = amount;
                                    }
                                }
                                citation.Currency = "USD";

                                // Issue Date (cell 3)
                                var dateText = cells[3].InnerText?.Trim();
                                if (DateTime.TryParse(dateText, out DateTime issueDate))
                                {
                                    citation.IssueDate = issueDate;
                                }

                                // License Plate / Tag (cell 4)
                                citation.Tag = plateNumber;

                                // State 
                                citation.State = stateCode;

                                // Location / Address (cell 5)
                                citation.Address = cells[5].InnerText?.Trim();

                                // Set agency from the page
                                citation.Agency = Agensy;

                                // Set Link to base URL
                                citation.Link = BaseUrl;

                                // Set as active
                                citation.IsActive = true;

                                citations.Add(citation);
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error parsing individual citation row: {ex.Message}");
                            // Continue processing other rows
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error parsing citations: {ex.Message}");
            }

            return citations;
        }
    }

    public class LinkInfo
    {
        public string Url { get; set; } = string.Empty;
        public string Text { get; set; } = string.Empty;

        public override string ToString()
        {
            return $"{Text}: {Url}";
        }
    }
 
}
