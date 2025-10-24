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

namespace HuurVioaltionsAPI
{
    public interface IHuurAPIFinder : IHuurViloationCaller
    {
        string Link { get; }
        Task<List<ParkingViolation>> Find(string licensePlate, string state);
        
        /// <summary>
        /// Event raised when an error occurs during the find operation
        /// </summary>
        event EventHandler<FinderErrorEventArgs>? Error;
    }

    /// <summary>
    /// Event arguments for finder errors
    /// </summary>
    public class FinderErrorEventArgs : EventArgs
    {
        public string FinderName { get; set; } = string.Empty;
        public string LicensePlate { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public Exception Exception { get; set; } = null!;
        public string Message { get; set; } = string.Empty;
    }
}
