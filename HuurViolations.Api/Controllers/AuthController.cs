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

using Microsoft.AspNetCore.Mvc;
using HuurViolations.Api.Services;
using HuurViolations.Api.Models;

namespace HuurViolations.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly ILogger<AuthController> _logger;
    private readonly ILocalJwtService _localJwtService;

    public AuthController(ILogger<AuthController> logger, ILocalJwtService localJwtService)
    {
        _logger = logger;
        _localJwtService = localJwtService;
    }

    /// <summary>
    /// Sign in with username and password
    /// </summary>
    /// <param name="request">Sign in credentials</param>
    /// <returns>JWT token if successful</returns>
    [HttpPost("signin")]
    [ProducesResponseType(typeof(SignInResponse), 200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> SignIn([FromBody] SignInRequest request)
    {
        _logger.LogInformation("Sign in request for user: {Username}", request.Username);

        // Simple validation - in production, validate against database
        if (string.IsNullOrEmpty(request.Username) || string.IsNullOrEmpty(request.Password))
        {
            return BadRequest(new SignInResponse 
            { 
                Success = false, 
                Message = "Username and password are required" 
            });
        }

        // For demo purposes, accept any username/password
        // In production, validate against user database
        var token = _localJwtService.GenerateToken(request.Username, "user");
        var expiresAt = DateTime.UtcNow.AddHours(1);

        _logger.LogInformation("User {Username} signed in successfully", request.Username);

        return Ok(new SignInResponse
        {
            Success = true,
            Token = token,
            Message = "Sign in successful",
            ExpiresAt = expiresAt
        });
    }

    /// <summary>
    /// Sign up a new user
    /// </summary>
    /// <param name="request">Sign up details</param>
    /// <returns>JWT token if successful</returns>
    [HttpPost("signup")]
    [ProducesResponseType(typeof(SignUpResponse), 200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> SignUp([FromBody] SignUpRequest request)
    {
        _logger.LogInformation("Sign up request for user: {Username}", request.Username);

        // Simple validation - in production, validate against database
        if (string.IsNullOrEmpty(request.Username) || string.IsNullOrEmpty(request.Password) || string.IsNullOrEmpty(request.Email))
        {
            return BadRequest(new SignUpResponse 
            { 
                Success = false, 
                Message = "Username, password, and email are required" 
            });
        }

        // For demo purposes, create user account
        // In production, save to database
        var token = _localJwtService.GenerateToken(request.Username, "user");
        var expiresAt = DateTime.UtcNow.AddHours(1);

        _logger.LogInformation("User {Username} signed up successfully", request.Username);

        return Ok(new SignUpResponse
        {
            Success = true,
            Token = token,
            Message = "Sign up successful",
            ExpiresAt = expiresAt
        });
    }

    /// <summary>
    /// Refresh an existing JWT token
    /// </summary>
    /// <param name="request">Refresh token request</param>
    /// <returns>New JWT token if successful</returns>
    [HttpPost("refresh")]
    [ProducesResponseType(typeof(RefreshTokenResponse), 200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        _logger.LogInformation("Token refresh request");

        if (string.IsNullOrEmpty(request.Token))
        {
            return BadRequest(new RefreshTokenResponse 
            { 
                Success = false, 
                Message = "Token is required" 
            });
        }

        // Validate the existing token
        if (!_localJwtService.ValidateToken(request.Token))
        {
            return BadRequest(new RefreshTokenResponse 
            { 
                Success = false, 
                Message = "Invalid token" 
            });
        }

        // Get user info from token and generate new token
        var principal = _localJwtService.GetPrincipalFromToken(request.Token);
        if (principal == null)
        {
            return BadRequest(new RefreshTokenResponse 
            { 
                Success = false, 
                Message = "Invalid token" 
            });
        }

        var username = principal.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value ?? "user";
        var role = principal.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value ?? "user";
        
        var newToken = _localJwtService.GenerateToken(username, role);
        var expiresAt = DateTime.UtcNow.AddHours(1);

        _logger.LogInformation("Token refreshed successfully for user: {Username}", username);

        return Ok(new RefreshTokenResponse
        {
            Success = true,
            Token = newToken,
            Message = "Token refreshed successfully",
            ExpiresAt = expiresAt
        });
    }
}
