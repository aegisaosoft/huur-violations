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

using System.Text;
using System.Text.Json;
using HuurApi.Models;

namespace HuurVioaltionsAPI
{
    public class HuurApiClient : IDisposable
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;
        private static readonly JsonSerializerOptions _jsonSerializerOptions = new() { PropertyNameCaseInsensitive = true };

        public HuurApiClient(string baseUrl)
        {
            ArgumentNullException.ThrowIfNull(baseUrl, nameof(baseUrl));
            _baseUrl = baseUrl.TrimEnd('/');
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
            var apiKey = Environment.GetEnvironmentVariable("HUUR_API_KEY");
            if (!string.IsNullOrWhiteSpace(apiKey))
            {
                _httpClient.DefaultRequestHeaders.Add("X-API-Key", apiKey);
            }
        }

        public async Task<bool> CreateViolationAsync(ParkingViolation violation)
        {
            var json = JsonSerializer.Serialize(violation, _jsonSerializerOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var url = $"{_baseUrl}/api/violations";
            try
            {
                var res = await _httpClient.PostAsync(url, content);
                return res.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        public async Task<List<ParkingViolation>> GetViolationsAsync()
        {
            var url = $"{_baseUrl}/api/violations";
            try
            {
                var response = await _httpClient.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    var jsonContent = await response.Content.ReadAsStringAsync();
                    var violations = JsonSerializer.Deserialize<List<ParkingViolation>>(jsonContent, _jsonSerializerOptions);
                    return violations ?? new List<ParkingViolation>();
                }
                return new List<ParkingViolation>();
            }
            catch
            {
                return new List<ParkingViolation>();
            }
        }

        public void Dispose()
        {
            _httpClient?.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}