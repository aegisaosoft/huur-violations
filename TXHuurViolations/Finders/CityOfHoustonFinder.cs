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

namespace TXHuurViolations.Finders
{
    internal class CityOfHoustonFinder : AHttpFinder, IHuurAPIFinder
    {
        protected static string _url = "https://houstonparking.t2hosted.com";
        protected PortalHelper helper = new PortalHelper(_url, "City of Houston");
        public string Name => "City of Houston";
        public string Link => _url;
        
        public event EventHandler<FinderErrorEventArgs>? Error;

        public async Task<List<ParkingViolation>> Find(string licensePlate, string state)
        {
            try
            {
                var violations = await helper.SearchCitation(licensePlate, state);
                
                // Set Link property for all violations
                foreach (var violation in violations)
                {
                    violation.Link = _url;
                }
                
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
    }
}
