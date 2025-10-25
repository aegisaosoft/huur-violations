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

using Microsoft.Extensions.DependencyInjection;

namespace HuurViolations.Auth;

/// <summary>
/// Extension methods for registering authentication services
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds HuurApi authentication services to the service collection
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddHuurApiAuthentication(this IServiceCollection services)
    {
        services.AddHttpClient<IAuthenticationService, HuurApiAuthenticationService>();
        return services;
    }
}
