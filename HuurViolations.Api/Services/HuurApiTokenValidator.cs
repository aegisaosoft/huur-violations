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

using System.Text.Json;

namespace HuurViolations.Api.Services;

public interface IHuurApiTokenValidator
{
    Task<bool> ValidateTokenAsync(string token);
}

public class HuurApiTokenValidator : IHuurApiTokenValidator
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<HuurApiTokenValidator> _logger;

    public HuurApiTokenValidator(HttpClient httpClient, IConfiguration configuration, ILogger<HuurApiTokenValidator> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<bool> ValidateTokenAsync(string token)
    {
        try
        {
            var huurApiUrl = _configuration["HUUR_API"];
            if (string.IsNullOrEmpty(huurApiUrl))
            {
                _logger.LogError("HUUR_API configuration is missing");
                return false;
            }

            var request = new HttpRequestMessage(HttpMethod.Get, $"{huurApiUrl.TrimEnd('/')}/Companies/active");
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            _logger.LogInformation("Validating token with HuurApi: {Url}", request.RequestUri);

            var response = await _httpClient.SendAsync(request);
            
            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Token validation successful");
                return true;
            }

            _logger.LogWarning("Token validation failed with status: {StatusCode}", response.StatusCode);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating token with HuurApi");
            return false;
        }
    }
}
