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

namespace HuurViolations.Auth;

/// <summary>
/// Interface for authentication service
/// </summary>
public interface IAuthenticationService
{
    /// <summary>
    /// Validates a Bearer token by calling the HuurApi
    /// </summary>
    /// <param name="token">The Bearer token to validate</param>
    /// <returns>Authentication result with validation status and company data</returns>
    Task<AuthenticationResult> ValidateTokenAsync(string token);

    /// <summary>
    /// Signs in a user and returns a JWT token
    /// </summary>
    /// <param name="request">Sign in request with credentials</param>
    /// <returns>Sign in result with token if successful</returns>
    Task<SignInResult> SignInAsync(SignInRequest request);

    /// <summary>
    /// Signs up a new user and returns a JWT token
    /// </summary>
    /// <param name="request">Sign up request with user details</param>
    /// <returns>Sign up result with token if successful</returns>
    Task<SignUpResult> SignUpAsync(SignUpRequest request);

    /// <summary>
    /// Refreshes an existing JWT token
    /// </summary>
    /// <param name="request">Refresh token request</param>
    /// <returns>Refresh result with new token if successful</returns>
    Task<RefreshTokenResult> RefreshTokenAsync(RefreshTokenRequest request);
}

/// <summary>
/// Result of token validation
/// </summary>
public class AuthenticationResult
{
    /// <summary>
    /// Whether the token is valid
    /// </summary>
    public bool IsValid { get; set; }
    
    /// <summary>
    /// Error message if validation failed
    /// </summary>
    public string? Error { get; set; }
    
    /// <summary>
    /// List of companies associated with the token
    /// </summary>
    public List<CompanyInfo> Companies { get; set; } = new List<CompanyInfo>();
}

/// <summary>
/// Company information from HuurApi
/// </summary>
public class CompanyInfo
{
    /// <summary>
    /// Company ID
    /// </summary>
    public int Id { get; set; }
    
    /// <summary>
    /// Company name
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Whether the company is active
    /// </summary>
    public bool IsActive { get; set; }
    
    /// <summary>
    /// Additional company properties
    /// </summary>
    public Dictionary<string, object> AdditionalProperties { get; set; } = new Dictionary<string, object>();
}

/// <summary>
/// Sign in request
/// </summary>
public class SignInRequest
{
    /// <summary>
    /// Username or email
    /// </summary>
    public string Username { get; set; } = string.Empty;
    
    /// <summary>
    /// Password
    /// </summary>
    public string Password { get; set; } = string.Empty;
}

/// <summary>
/// Sign in result
/// </summary>
public class SignInResult
{
    /// <summary>
    /// Whether sign in was successful
    /// </summary>
    public bool IsSuccess { get; set; }
    
    /// <summary>
    /// JWT token if successful
    /// </summary>
    public string? Token { get; set; }
    
    /// <summary>
    /// Token expiration time
    /// </summary>
    public DateTime? ExpiresAt { get; set; }
    
    /// <summary>
    /// Error message if failed
    /// </summary>
    public string? Error { get; set; }
}

/// <summary>
/// Sign up request
/// </summary>
public class SignUpRequest
{
    /// <summary>
    /// Username
    /// </summary>
    public string Username { get; set; } = string.Empty;
    
    /// <summary>
    /// Email address
    /// </summary>
    public string Email { get; set; } = string.Empty;
    
    /// <summary>
    /// Password
    /// </summary>
    public string Password { get; set; } = string.Empty;
    
    /// <summary>
    /// Full name
    /// </summary>
    public string FullName { get; set; } = string.Empty;
}

/// <summary>
/// Sign up result
/// </summary>
public class SignUpResult
{
    /// <summary>
    /// Whether sign up was successful
    /// </summary>
    public bool IsSuccess { get; set; }
    
    /// <summary>
    /// JWT token if successful
    /// </summary>
    public string? Token { get; set; }
    
    /// <summary>
    /// Token expiration time
    /// </summary>
    public DateTime? ExpiresAt { get; set; }
    
    /// <summary>
    /// Error message if failed
    /// </summary>
    public string? Error { get; set; }
}

/// <summary>
/// Refresh token request
/// </summary>
public class RefreshTokenRequest
{
    /// <summary>
    /// Current JWT token
    /// </summary>
    public string Token { get; set; } = string.Empty;
}

/// <summary>
/// Refresh token result
/// </summary>
public class RefreshTokenResult
{
    /// <summary>
    /// Whether refresh was successful
    /// </summary>
    public bool IsSuccess { get; set; }
    
    /// <summary>
    /// New JWT token if successful
    /// </summary>
    public string? Token { get; set; }
    
    /// <summary>
    /// Token expiration time
    /// </summary>
    public DateTime? ExpiresAt { get; set; }
    
    /// <summary>
    /// Error message if failed
    /// </summary>
    public string? Error { get; set; }
}
