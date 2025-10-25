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

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace HuurViolations.Auth;

/// <summary>
/// Authentication service that validates tokens against HuurApi
/// </summary>
public class HuurApiAuthenticationService : IAuthenticationService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<HuurApiAuthenticationService> _logger;

    public HuurApiAuthenticationService(
        HttpClient httpClient, 
        IConfiguration configuration, 
        ILogger<HuurApiAuthenticationService> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;
    }

    /// <summary>
    /// Validates a Bearer token by calling the HuurApi Companies/active endpoint
    /// </summary>
    /// <param name="token">The Bearer token to validate</param>
    /// <returns>Authentication result with validation status</returns>
    public async Task<AuthenticationResult> ValidateTokenAsync(string token)
    {
        try
        {
            // Get HuurApi URL from configuration
            var huurApiUrl = _configuration["HUUR_API"] ?? "https://agsm-back.azurewebsites.net/";
            
            _logger.LogInformation("Validating token against HuurApi at: {Url}", huurApiUrl);

            // Create request to get active companies list (requires _admin role)
            var request = new HttpRequestMessage(HttpMethod.Get, $"{huurApiUrl.TrimEnd('/')}/Companies/active");
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            
            _logger.LogInformation("Calling HuurApi Companies/active endpoint to validate token");
            
            var response = await _httpClient.SendAsync(request);
            
            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Token is valid - HuurApi call successful: {StatusCode}", response.StatusCode);
                
                return new AuthenticationResult 
                { 
                    IsValid = true
                };
            }
            else
            {
                _logger.LogWarning("Token is invalid - HuurApi call failed: {StatusCode} - {ReasonPhrase}", 
                    response.StatusCode, response.ReasonPhrase);
                
                return new AuthenticationResult 
                { 
                    IsValid = false, 
                    Error = $"Token validation failed: {response.StatusCode} - {response.ReasonPhrase}" 
                };
            }
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Network error during token validation - token is invalid");
            return new AuthenticationResult 
            { 
                IsValid = false, 
                Error = $"Network error: {ex.Message}" 
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during token validation - token is invalid");
            return new AuthenticationResult 
            { 
                IsValid = false, 
                Error = $"Unexpected error: {ex.Message}" 
            };
        }
    }

    /// <summary>
    /// Signs in a user and returns a JWT token
    /// </summary>
    /// <param name="request">Sign in request with credentials</param>
    /// <returns>Sign in result with token if successful</returns>
    public async Task<SignInResult> SignInAsync(SignInRequest request)
    {
        try
        {
            _logger.LogInformation("Attempting sign in for user: {Username}", request.Username);

            // Validate credentials (in a real app, this would check against a database)
            if (string.IsNullOrEmpty(request.Username) || string.IsNullOrEmpty(request.Password))
            {
                return new SignInResult
                {
                    IsSuccess = false,
                    Error = "Username and password are required"
                };
            }

            // For demo purposes, accept any non-empty credentials
            // In a real application, you would validate against a user database
            if (request.Username.Length < 3 || request.Password.Length < 6)
            {
                return new SignInResult
                {
                    IsSuccess = false,
                    Error = "Invalid credentials"
                };
            }

            // Generate JWT token
            var token = GenerateJwtToken(request.Username);
            var expiresAt = DateTime.UtcNow.AddHours(24); // Token expires in 24 hours

            _logger.LogInformation("Sign in successful for user: {Username}", request.Username);

            return new SignInResult
            {
                IsSuccess = true,
                Token = token,
                ExpiresAt = expiresAt
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during sign in for user: {Username}", request.Username);
            return new SignInResult
            {
                IsSuccess = false,
                Error = "Internal server error during sign in"
            };
        }
    }

    /// <summary>
    /// Signs up a new user and returns a JWT token
    /// </summary>
    /// <param name="request">Sign up request with user details</param>
    /// <returns>Sign up result with token if successful</returns>
    public async Task<SignUpResult> SignUpAsync(SignUpRequest request)
    {
        try
        {
            _logger.LogInformation("Attempting sign up for user: {Username}", request.Username);

            // Validate input
            if (string.IsNullOrEmpty(request.Username) || string.IsNullOrEmpty(request.Email) || 
                string.IsNullOrEmpty(request.Password) || string.IsNullOrEmpty(request.FullName))
            {
                return new SignUpResult
                {
                    IsSuccess = false,
                    Error = "All fields are required"
                };
            }

            // Validate email format
            if (!request.Email.Contains("@"))
            {
                return new SignUpResult
                {
                    IsSuccess = false,
                    Error = "Invalid email format"
                };
            }

            // Validate password strength
            if (request.Password.Length < 6)
            {
                return new SignUpResult
                {
                    IsSuccess = false,
                    Error = "Password must be at least 6 characters long"
                };
            }

            // In a real application, you would check if user already exists and create new user
            // For demo purposes, we'll accept any valid input

            // Generate JWT token
            var token = GenerateJwtToken(request.Username);
            var expiresAt = DateTime.UtcNow.AddHours(24); // Token expires in 24 hours

            _logger.LogInformation("Sign up successful for user: {Username}", request.Username);

            return new SignUpResult
            {
                IsSuccess = true,
                Token = token,
                ExpiresAt = expiresAt
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during sign up for user: {Username}", request.Username);
            return new SignUpResult
            {
                IsSuccess = false,
                Error = "Internal server error during sign up"
            };
        }
    }

    /// <summary>
    /// Refreshes an existing JWT token
    /// </summary>
    /// <param name="request">Refresh token request</param>
    /// <returns>Refresh result with new token if successful</returns>
    public async Task<RefreshTokenResult> RefreshTokenAsync(RefreshTokenRequest request)
    {
        try
        {
            _logger.LogInformation("Attempting to refresh token");

            if (string.IsNullOrEmpty(request.Token))
            {
                return new RefreshTokenResult
                {
                    IsSuccess = false,
                    Error = "Token is required"
                };
            }

            // Validate the existing token
            var authResult = await ValidateTokenAsync(request.Token);
            if (!authResult.IsValid)
            {
                return new RefreshTokenResult
                {
                    IsSuccess = false,
                    Error = "Invalid token provided"
                };
            }

            // Extract username from token (in a real app, you'd parse the JWT)
            // For demo purposes, we'll generate a new token
            var username = "user"; // In real app, extract from JWT claims
            var newToken = GenerateJwtToken(username);
            var expiresAt = DateTime.UtcNow.AddHours(24); // Token expires in 24 hours

            _logger.LogInformation("Token refresh successful");

            return new RefreshTokenResult
            {
                IsSuccess = true,
                Token = newToken,
                ExpiresAt = expiresAt
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during token refresh");
            return new RefreshTokenResult
            {
                IsSuccess = false,
                Error = "Internal server error during token refresh"
            };
        }
    }

    /// <summary>
    /// Generates a JWT token for the given username
    /// </summary>
    /// <param name="username">Username to include in the token</param>
    /// <returns>JWT token string</returns>
    private string GenerateJwtToken(string username)
    {
        var jwtKey = _configuration["JWT_SECRET_KEY"] ?? "your-super-secret-jwt-key-that-should-be-at-least-32-characters-long";
        var key = Encoding.ASCII.GetBytes(jwtKey);

        var tokenDescriptor = new Microsoft.IdentityModel.Tokens.SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Name, username),
                new Claim(ClaimTypes.NameIdentifier, username),
                new Claim("role", "_admin") // Give admin role for HuurApi access
            }),
            Expires = DateTime.UtcNow.AddHours(24),
            SigningCredentials = new Microsoft.IdentityModel.Tokens.SigningCredentials(
                new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(key),
                Microsoft.IdentityModel.Tokens.SecurityAlgorithms.HmacSha256Signature)
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

}
